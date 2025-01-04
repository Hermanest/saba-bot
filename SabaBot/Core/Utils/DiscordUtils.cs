using Discord;

namespace SabaBot.Utils;

internal static class DiscordUtils {
    public static bool TryParseEmote(string? str, out IEmote? emote) {
        emote = null;
        if (str == null) {
            return false;
        }
        if (Emote.TryParse(str, out var result1)) {
            emote = result1;
            return true;
        }
        if (!Emoji.TryParse(str, out var result2)) return false;
        emote = result2;
        return true;
    }
}