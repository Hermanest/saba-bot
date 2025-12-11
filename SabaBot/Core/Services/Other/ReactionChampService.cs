using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using SabaBot.Database;
using SabaBot.Utils;

namespace SabaBot;

public class ReactionChampService(
    DiscordSocketClient client,
    IDbContextFactory<ApplicationContext> contextFactory,
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

        var (settings, context) = await LoadGuildSettings(message);
        if (settings == null) {
            return "This command can be used only on guild channels.";
        }

        var emote = LoadEmote(settings.ReactionChampSettings);
        await AddMessageInternal(message, emote, settings, context!);
        return null;
    }

    private async Task AddMessageInternal(IUserMessage message, IEmote? emote, GuildSettings guildSettings, ApplicationContext context) {
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

        var (guildSettings, context) = await LoadGuildSettings(message);
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

        await AddMessageInternal(message, emote!, guildSettings, context);
    }

    private async Task<(GuildSettings?, ApplicationContext?)> LoadGuildSettings(IUserMessage message) {
        if (message.Author is not IGuildUser user) {
            return (null, null);
        }

        var ctx = await contextFactory.CreateDbContextAsync();
        var res = await ctx.EnsureSettingsCreated(user.GuildId);

        return (res, ctx);
    }

    private static IEmote? LoadEmote(ReactionChampSettings settings) {
        if (!settings.Enabled || !DiscordUtils.TryParseEmote(settings.EmoteId, out var emote)) {
            return null;
        }
        return emote;
    }
}