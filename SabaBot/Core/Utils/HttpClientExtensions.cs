using Microsoft.Extensions.Logging;

namespace SabaBot.Utils;

internal static class HttpClientExtensions {
    public static async Task<Stream?> TryOpenStreamAsync(this HttpClient client, string url, ILogger? logger = null) {
        try {
            var stream = await client.GetStreamAsync(url);
            return stream.CanRead ? stream : null;
        } catch (Exception e) {
            logger?.LogError(e.ToString());
            return null;
        }
    }
}