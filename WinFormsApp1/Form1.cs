using System;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;
using AMRichPresence;
using DiscordRPC;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        private CancellationTokenSource? rpcTokenSource;
        private Task? rpcTask;
        private DiscordRpcClient? rpcClient;

        private AppleMusicInfo? lastSongInfo = null;

        public Form1()
        {
            InitializeComponent();
            AMRPC.DoubleClick += notifyIcon1_DoubleClick;
        }

        // Start Rich Presence
        private void button1_Click(object sender, EventArgs e)
        {
            if (rpcTask != null && !rpcTask.IsCompleted)
            {
                MessageBox.Show(
                    "Rich Presence is already running.",
                    "AMRichPresence",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                return;
            }

            rpcTokenSource = new CancellationTokenSource();
            rpcTask = Task.Run(() => RunRichPresenceLoop(rpcTokenSource.Token));
            MessageBox.Show(
                "Rich Presence started!\n\nNOTE: If you have stopped it and then started it again with the same song, please click a different song and go back to your original song.",
                "AMRichPresence",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            button1_Click(sender, e);
        }

        // Stop Rich Presence
        private async void button2_Click(object sender, EventArgs e)
        {
            if (rpcTokenSource != null)
            {
                rpcTokenSource.Cancel();
                if (rpcTask != null)
                {
                    try { await rpcTask; } catch { }
                    rpcTask = null;
                }
                rpcTokenSource = null;
            }
            MessageBox.Show(
                "Rich Presence stopped.",
                "AMRichPresence",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            button2_Click(sender, e);
        }

        // Minimize to tray
        private void button3_Click(object sender, EventArgs e)
        {
            this.Hide();
            AMRPC.Visible = true;
        }

        private void button3_Click_2(object sender, EventArgs e)
        {
            button3_Click(sender, e);
        }

        // Restore from tray
        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            AMRPC.Visible = false;
        }

        private async Task RunRichPresenceLoop(CancellationToken cancellationToken)
        {
            rpcClient = new DiscordRpcClient("1371653005945995427");
            rpcClient.Initialize();

            var scraper = new AppleMusicClientScraper();

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var info = await scraper.GetAppleMusicInfo();
                        bool songLooped = false;

                        if (info != null && lastSongInfo != null)
                        {
                            // Detect song restart (loop): song name and artist same, but playback time jumps back
                            if (info.SongName == lastSongInfo.SongName &&
                                info.SongArtist == lastSongInfo.SongArtist &&
                                info.CurrentTime.HasValue && lastSongInfo.CurrentTime.HasValue &&
                                info.CurrentTime.Value < lastSongInfo.CurrentTime.Value - 2)
                            {
                                songLooped = true;

                                // Show tray notification (not a popup)
                                AMRPC.ShowBalloonTip(
                                    3000, // duration in ms
                                    "Song Looped",
                                    $"\"{info.SongName}\" by {info.SongArtist} started over.",
                                    ToolTipIcon.Info
                                );
                            }
                        }

                        // Only update Rich Presence if song changed or song looped
                        if (info == null || info.IsPaused || string.IsNullOrWhiteSpace(info.SongName) ||
                            string.IsNullOrWhiteSpace(info.SongArtist) || !info.PlaybackStart.HasValue || !info.PlaybackEnd.HasValue)
                        {
                            if (rpcClient.IsInitialized)
                            {
                                rpcClient.ClearPresence();
                            }
                        }
                        else if (songLooped ||
                                 info.SongName != lastSongInfo?.SongName ||
                                 info.SongArtist != lastSongInfo?.SongArtist ||
                                 info.SongAlbum != lastSongInfo?.SongAlbum)
                        {
                            SetRichPresence(rpcClient, info);
                        }

                        lastSongInfo = info;
                    }
                    catch
                    {
                        // Optionally log errors here
                    }
                    await Task.Delay(1000, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                // This is expected when stopping, do nothing
            }
            finally
            {
                if (rpcClient.IsInitialized)
                    rpcClient.ClearPresence();
                rpcClient.Dispose();
                rpcClient = null;
            }
        }

        private void SetRichPresence(DiscordRpcClient rpc, AppleMusicInfo info)
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
            catch
            {
            }
        }

        private void label1_Click(object sender, EventArgs e) { }
        private void label2_Click(object sender, EventArgs e) { }
        private void pictureBox1_Click(object sender, EventArgs e) { }
        private void pictureBox1_Click_1(object sender, EventArgs e) { }
        private void contextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e) { }
        private void Form1_Load(object sender, EventArgs e) { }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "https://github.com/sponge104/AMRichPresence",
                UseShellExecute = true
            });
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "https://github.com/sponge104/",
                UseShellExecute = true
            });
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {

        }
    }
}