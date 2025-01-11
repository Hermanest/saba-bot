using Discord;
using Discord.WebSocket;
using SabaBot.Database;
using SabaBot.Utils;

namespace SabaBot;

public class ReactionChampService(
    DiscordSocketClient client,
    ApplicationContext context,
    ILocalization localization
) : IService, IDisposable {
    public void Start() {
        client.ReactionAdded += HandleReactionAdded;
    }

    public void Dispose() {
        client.ReactionAdded -= HandleReactionAdded;
    }

    public async Task<string?> AddMessage(IUserMessage message) {
        if (message.Content.IsNullOrEmpty()) {
            return "Message cannot be empty.";
        }

        if (message.Author.IsBot) {
            return "Message must belong to a user.";
        }
        
        var settings = await LoadGuildSettings(message);
        if (settings == null) {
            return "This command can be used only on guild channels.";
        }
        
        var emote = LoadEmote(settings.ReactionChampSettings);
        await AddMessageInternal(message, emote, settings);
        return null;
    }

    private async Task AddMessageInternal(IUserMessage message, IEmote? emote, GuildSettings guildSettings) {
        //formatting the message
        var key = localization[guildSettings.Locale, "ChampRemovedMessage"];
        var str = string.Format(key, message.Author.Mention, emote?.ToString() ?? "");

        //starting tasks
        var deleteTask = message.DeleteAsync();
        var sendTask = message.Channel.SendMessageAsync(str);

        //caching the message
        var cachedMessage = new RewindMessage {
            Text = message.Content,
            AuthorId = message.Author.Id
        };
        guildSettings.ReactionChampSettings.DeletedMessages.Shift(cachedMessage, 100);

        //waiting for the tasks to finish
        await context.SaveChangesAsync();
        await deleteTask;
        await sendTask;
    }

    private async Task HandleReactionAdded(
        Cacheable<IUserMessage, ulong> msg,
        Cacheable<IMessageChannel, ulong> channel,
        SocketReaction reaction
    ) {
        if (reaction.Message is not { IsSpecified: true } m) {
            return;
        }

        var message = m.Value;
        if (message.Author.IsBot || message.Content.IsNullOrEmpty()) {
            return;
        }
        
        var guildSettings = await LoadGuildSettings(message);
        if (guildSettings == null) {
            return;
        }

        var settings = guildSettings.ReactionChampSettings;
        var emote = LoadEmote(settings);
        if (emote == null) {
            return;
        }
        
        if (!message.Reactions.TryGetValue(emote, out var meta) || meta.ReactionCount < settings.ReactionThreshold) {
            return;
        }

        await AddMessageInternal(message, emote!, guildSettings);
    }

    private async Task<GuildSettings?> LoadGuildSettings(IUserMessage message) {
        if (message.Author is not IGuildUser user) {
            return null;
        }
        return await context.EnsureSettingsCreated(user.GuildId);
    }

    private static IEmote? LoadEmote(ReactionChampSettings settings) {
        if (!settings.Enabled || !DiscordUtils.TryParseEmote(settings.EmoteId, out var emote)) {
            return null;
        }
        return emote;
    }
}