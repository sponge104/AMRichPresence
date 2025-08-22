using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using DiscordRPC;
using System.Net.Http;
using System.Text.Json;

namespace AMRichPresence
{
    public class AppleMusicInfo
    {
        public string SongName;
        public string SongAlbum;
        public string SongArtist;
        public bool IsPaused = true;
        public DateTime? PlaybackStart;
        public DateTime? PlaybackEnd;
        public int? SongDuration = null;
        public string? CoverArtUrl = null;
        public int? CurrentTime = null;
        public AppleMusicInfo(string songName, string songAlbum, string songArtist)
        {
            SongName = songName;
            SongAlbum = songAlbum;
            SongArtist = songArtist;
        }
    }

    public class AppleMusicClientScraper
    {
        public async Task<AppleMusicInfo?> GetAppleMusicInfo()
        {
            var amProcesses = Process.GetProcessesByName("AppleMusic");
            if (!amProcesses.Any()) return null;

            var windows = new List<AutomationElement>();
            using (var automation = new UIA3Automation())
            {
                var processId = amProcesses[0].Id;
                windows.AddRange(automation.GetDesktop().FindAllChildren(c => c.ByProcessId(processId)));
            }

            AutomationElement? amSongPanel = null;
            bool isMiniPlayer = false;
            foreach (var window in windows)
            {
                isMiniPlayer = window.Name == "Mini Player";
                if (isMiniPlayer)
                {
                    amSongPanel = window.FindFirstDescendant(cf => cf.ByClassName("InputSiteWindowClass"));
                    if (amSongPanel != null) break;
                }
                else
                {
                    amSongPanel = window.FindFirstDescendant(cf => cf.ByAutomationId("TransportBar")) ?? amSongPanel;
                }
            }
            if (amSongPanel == null) return null;

            var songFieldsPanel = isMiniPlayer ? amSongPanel : amSongPanel.FindFirstChild("LCD");
            var songFields = songFieldsPanel?.FindAllChildren(cf => cf.ByAutomationId("myScrollViewer")) ?? Array.Empty<AutomationElement>();
            if (!isMiniPlayer && songFields.Length != 2) return null;

            var songNameElement = songFields[0];
            var songAlbumArtistElement = songFields[1];
            if (songNameElement.BoundingRectangle.Bottom > songAlbumArtistElement.BoundingRectangle.Bottom)
            {
                songNameElement = songFields[1];
                songAlbumArtistElement = songFields[0];
            }
            var songName = songNameElement.Name;
            var songAlbumArtist = songAlbumArtistElement.Name;

            // Parse artist/album (with emdash logic)
            string songArtist, songAlbum;
            ParseSongAlbumArtist(songAlbumArtist, out songArtist, out songAlbum);

            var info = new AppleMusicInfo(songName, songAlbum, songArtist);

            // Paused detection
            var playPauseButton = amSongPanel.FindFirstChild("TransportControl_PlayPauseStop");
            var songProgressSlider = (isMiniPlayer ? amSongPanel.FindFirstChild("Scrubber") : amSongPanel.FindFirstChild("LCD")?.FindFirstChild("LCDScrubber"))?.Patterns.RangeValue.Pattern;
            var songProgressPercent = songProgressSlider == null ? 0 : songProgressSlider.Value / songProgressSlider.Maximum;
            if (playPauseButton?.Name == "Play" || playPauseButton?.Name == "Pause")
                info.IsPaused = playPauseButton.Name == "Play";
            else
                info.IsPaused = songProgressSlider != null && songProgressSlider.Value == 0;

            // Timestamp extraction (Mini Player or main UI)
            var currentTimeElement = songFieldsPanel?.FindFirstChild("CurrentTime");
            var remainingDurationElement = songFieldsPanel?.FindFirstChild("Duration");
            int? currentTime = null;
            int? remainingDuration = null;
            bool gotUITimestamps = false;
            if (currentTimeElement != null && remainingDurationElement != null)
            {
                currentTime = ParseTimeString(currentTimeElement.Name);
                remainingDuration = ParseTimeString(remainingDurationElement.Name);
                if (currentTime != null && remainingDuration != null)
                {
                    gotUITimestamps = true;
                    info.CurrentTime = currentTime;
                    info.PlaybackStart = DateTime.UtcNow - TimeSpan.FromSeconds(currentTime.Value);
                    info.PlaybackEnd = DateTime.UtcNow + TimeSpan.FromSeconds(remainingDuration.Value);
                }
            }

            // If timestamps not available, try slider + web API for duration (AMWin-RP style)
            if (!gotUITimestamps)
            {
                int? duration = await FetchSongDuration(songName, songArtist);
                if (duration != null && songProgressSlider != null)
                {
                    int progress = (int)(duration.Value * songProgressPercent);
                    int remaining = duration.Value - progress;
                    info.SongDuration = duration;
                    info.CurrentTime = progress;
                    info.PlaybackStart = DateTime.UtcNow - TimeSpan.FromSeconds(progress);
                    info.PlaybackEnd = DateTime.UtcNow + TimeSpan.FromSeconds(remaining);
                }
            }

            // Cover art (optional, can comment out if not wanted)
            info.CoverArtUrl = await FetchCoverArtUrl(songName, songArtist);

            return info;
        }

        private static void ParseSongAlbumArtist(string songAlbumArtist, out string songArtist, out string songAlbum)
        {
            var songSplit = songAlbumArtist.Split(new[] { " \u2014 " }, StringSplitOptions.None);
            if (songSplit.Length > 1)
            {
                songArtist = songSplit[0];
                songAlbum = songSplit[1];
            }
            else
            {
                songArtist = songSplit[0];
                songAlbum = songSplit[0];
            }
        }

        // Parses 1:23 or -1:23 to seconds (removes - if present)
        public static int? ParseTimeString(string? time)
        {
            if (string.IsNullOrWhiteSpace(time)) return null;
            if (time.Contains('-')) time = time.Replace("-", "");
            var parts = time.Split(':');
            if (parts.Length == 2 && int.TryParse(parts[0], out int min) && int.TryParse(parts[1], out int sec))
                return min * 60 + sec;
            if (parts.Length == 3 && int.TryParse(parts[0], out int hr) && int.TryParse(parts[1], out int min2) && int.TryParse(parts[2], out int sec2))
                return hr * 3600 + min2 * 60 + sec2;
            return null;
        }

        // Fetch duration from iTunes web (AMWin-RP style)
        public static async Task<int?> FetchSongDuration(string song, string artist)
        {
            try
            {
                string searchTerm = Uri.EscapeDataString($"{song} {artist}");
                string url = $"https://itunes.apple.com/search?term={searchTerm}&entity=song&limit=1";
                using (HttpClient client = new HttpClient())
                {
                    string json = await client.GetStringAsync(url);
                    using (JsonDocument doc = JsonDocument.Parse(json))
                    {
                        var root = doc.RootElement;
                        if (root.TryGetProperty("results", out var results) && results.GetArrayLength() > 0)
                        {
                            var item = results[0];
                            if (item.TryGetProperty("trackTimeMillis", out var millisProp))
                            {
                                int millis = millisProp.GetInt32();
                                return millis / 1000;
                            }
                        }
                    }
                }
            }
            catch { }
            return null;
        }

        // Fetch cover art from iTunes web (AMWin-RP style)
        public static async Task<string?> FetchCoverArtUrl(string song, string artist)
        {
            try
            {
                string searchTerm = Uri.EscapeDataString($"{song} {artist}");
                string url = $"https://itunes.apple.com/search?term={searchTerm}&entity=song&limit=1";
                using (HttpClient client = new HttpClient())
                {
                    string json = await client.GetStringAsync(url);
                    using (JsonDocument doc = JsonDocument.Parse(json))
                    {
                        var root = doc.RootElement;
                        if (root.TryGetProperty("results", out var results) && results.GetArrayLength() > 0)
                        {
                            var item = results[0];
                            if (item.TryGetProperty("artworkUrl100", out var artUrlProp))
                            {
                                string artUrl = artUrlProp.GetString();
                                artUrl = artUrl.Replace("100x100bb", "600x600bb");
                                return artUrl;
                            }
                        }
                    }
                }
            }
            catch { }
            return null;
        }
    }

    class apple_music_rpc2
    {
        static async Task Main(string[] args)
        {
            var rpc = new DiscordRpcClient("1371653005945995427");
            rpc.Initialize();
            Console.WriteLine("[INFO] Discord RPC Initialized.");

            var scraper = new AppleMusicClientScraper();

            while (true)
            {
                try
                {
                    var info = await scraper.GetAppleMusicInfo();
                    if (info == null || info.IsPaused || string.IsNullOrWhiteSpace(info.SongName) ||
                        string.IsNullOrWhiteSpace(info.SongArtist) || !info.PlaybackStart.HasValue || !info.PlaybackEnd.HasValue)
                    {
                        if (rpc.IsInitialized)
                        {
                            rpc.ClearPresence();
                        }
                        Console.WriteLine("[INFO] Presence cleared (paused or missing/invalid info).");
                    }
                    else
                    {
                        SetRichPresence(rpc, info);
                        Console.WriteLine($"[DEBUG] Set presence: {info.SongName} - {info.SongArtist} | {info.SongAlbum}");
                        Console.WriteLine($"[DEBUG] Timestamps: start={info.PlaybackStart:O}, end={info.PlaybackEnd:O}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Exception in main loop: {ex}");
                }
                await Task.Delay(1000);
            }
        }

        public static void SetRichPresence(DiscordRpcClient rpc, AppleMusicInfo info)
        {
            if (!rpc.IsInitialized || info.PlaybackStart == null || info.PlaybackEnd == null) return;

            var presence = new RichPresence()
            {
                Details = info.SongName,
                State = info.SongArtist,
                Type = ActivityType.Listening,
                Assets = new Assets()
                {
                    LargeImageKey = info.CoverArtUrl ?? "applemusic1024x",
                    LargeImageText = string.IsNullOrWhiteSpace(info.SongAlbum) ? "Listening on Apple Music" : info.SongAlbum
                },
                Buttons = new[]
                {
                    new DiscordRPC.Button
                    {
                        Label = "Listen on Apple Music",
                        Url = "https://music.apple.com"
                    }
                },
                Timestamps = new Timestamps()
                {
                    Start = info.PlaybackStart.Value,
                    End = info.PlaybackEnd.Value
                }
            };
            try
            {
                rpc.SetPresence(presence);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to set Discord Rich Presence: {ex.Message}");
            }
        }
    }
}s