using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using SabaBot.Database;

namespace SabaBot;

internal class MessageService(
    DiscordSocketClient client,
    IDbContextFactory<ApplicationContext> contextFactory,
    IChatBot chatBot
) : IService {
    public void Start() {
        client.MessageReceived += HandleMessageReceived;
    }

    public void Dispose() {
        client.MessageReceived -= HandleMessageReceived;
    }

    private async Task<(RewindSettings, ApplicationContext)> GetSettings(ulong guildId) {
        var context = await contextFactory.CreateDbContextAsync();
        var settings = await context.EnsureSettingsCreated(guildId);
        
        return (settings.RewindSettings, context);
    }

    private async Task HandleMessageReceived(SocketMessage message) {
        if (message is not SocketUserMessage userMessage || message.Author is not IGuildUser user) {
            return;
        }

        if (message.Author.IsBot || message.MentionedEveryone) {
            return;
        }

        // Checking
        var (settings, context) = await GetSettings(user.Guild.Id);
        var mentioned = message.MentionedUsers.Any(x => x.Id == client.CurrentUser.Id);
        var replyResult = await CheckReplyNeeded(settings, userMessage, mentioned);
        var autoReplyResult = await CheckAutomaticReplyNeeded(settings, userMessage);
        
        // If both failed
        if (!replyResult && !autoReplyResult && !mentioned) {
            AddRewindMessage(settings, userMessage);
        }
        
        // Saving changes
        // TODO: possible perf issues. Keep a scoped context and refresh it over time.
        await context.SaveChangesAsync();
    }

    private async Task<bool> CheckAutomaticReplyNeeded(RewindSettings settings, IUserMessage message) {
        if (settings.EnableAutomaticReplies && settings.CooldownCounter >= settings.AutomaticRepliesCooldown) {
            await Reply(settings, message);
            return true;
        }
        settings.CooldownCounter++;
        return false;
    }

    private async Task<bool> CheckReplyNeeded(RewindSettings settings, IUserMessage message, bool mentioned) {
        if (!settings.EnablePingReplies || !mentioned) return false;
        await Reply(settings, message);
        return true;
    }

    private async Task Reply(RewindSettings settings, IUserMessage message) {
        settings.CooldownCounter = 0;
        await chatBot.ReplyAsync(settings, message);
    }

    private static void AddRewindMessage(RewindSettings settings, IUserMessage message) {
        if (string.IsNullOrEmpty(message.Content)) return;
        var rewindMessage = new RewindMessage {
            Text = message.Content,
            AuthorId = message.Author.Id
        };
        var messages = settings.Messages;
        if (messages.Count + 1 > settings.MessagesThreshold) {
            messages.RemoveAt(0);
        }
        messages.Add(rewindMessage);
    }
}