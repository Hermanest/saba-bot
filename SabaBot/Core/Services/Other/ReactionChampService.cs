using Discord;
using Discord.WebSocket;
using SabaBot.Database;
using SabaBot.Utils;

namespace SabaBot;

internal class ReactionChampService(
    DiscordSocketClient client,
    ApplicationContext context,
    ILocalization localization
) : IService, IDisposable {
    public void Start() {
        client.ReactionAdded += HandleReactiveAdded;
    }

    public void Dispose() {
        client.ReactionAdded -= HandleReactiveAdded;
    }

    private async Task HandleReactiveAdded(
        Cacheable<IUserMessage, ulong> msg,
        Cacheable<IMessageChannel, ulong> channel,
        SocketReaction reaction
    ) {
        if (reaction.Message is not { IsSpecified: true } m) {
            return;
        }
        var message = m.Value;
        if (message.Author is not IGuildUser user || message.Content.Length == 0) {
            return;
        }
        //
        var guildSettings = await context.EnsureSettingsCreated(user.GuildId);
        var settings = guildSettings.ReactionChampSettings;
        if (!settings.Enabled || !DiscordUtils.TryParseEmote(settings.EmoteId, out var emote)) return;
        //
        if (!message.Reactions.TryGetValue(emote!, out var metadata)) return;
        //
        if (metadata.ReactionCount < settings.ReactionThreshold) return;
        
        //formatting the message
        var key = localization[guildSettings.Locale, "ChampRemovedMessage"];
        var str = string.Format(key, message.Author.Mention, emote);
        
        //starting tasks
        var deleteTask = message.DeleteAsync();
        var sendTask = message.Channel.SendMessageAsync(str);
        
        //caching the message
        var cachedMessage = new RewindMessage {
            Text = message.Content,
            AuthorId = message.Author.Id
        };
        settings.DeletedMessages.Shift(cachedMessage, 100);
        
        //waiting for the tasks to finish
        await context.SaveChangesAsync();
        await deleteTask;
        await sendTask;
    }
}