using Discord.WebSocket;
using static Discord.GatewayIntents;

namespace SabaBot;

public record ApplicationConfig(
    string Token,
    string DbAddress,
    string LlamaAddress,
    string LlamaModelId,
    string LocalizationFile,
    string ResourcesPath
) {
    public static readonly DiscordSocketConfig DiscordConfig = new() {
        GatewayIntents = AllUnprivileged | MessageContent | GuildMembers,
        MessageCacheSize = 1000
    };
}