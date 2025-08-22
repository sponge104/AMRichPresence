using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

public static class fetch_cover_art2
{
    /// <summary>
    /// Fetches the iTunes cover art URL for the given song & artist.
    /// </summary>
    /// <param name="song">Song name</param>
    /// <param name="artist">Artist name</param>
    /// <returns>image url or null if not found</returns>
    public static async Task<string?> FetchCoverArtUrl(string song, string artist)
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
        return null;
    }
}