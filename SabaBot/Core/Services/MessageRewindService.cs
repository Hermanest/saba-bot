using Discord;
using Discord.WebSocket;
using SabaBot.Database;

namespace SabaBot;

internal class MessageRewindService(DiscordSocketClient client, ApplicationContext context) : IService {
    public void Start() {
        client.MessageReceived += HandleMessageReceived;
    }

    public void Dispose() {
        client.MessageReceived -= HandleMessageReceived;
    }

    private async Task<RewindSettings> GetSettings(ulong guildId) {
        await context.EnsureSettingsCreated(guildId);
        var settings = await context.Guilds.FindAsync(guildId);
        return settings!.RewindSettings;
    }

    private async Task HandleMessageReceived(SocketMessage message) {
        if (message is not SocketUserMessage userMessage || message.Author is not IGuildUser user) return;
        if (message.Author.Id == client.CurrentUser.Id) return;
        //checking
        var settings = await GetSettings(user.Guild.Id);
        var mentioned = message.MentionedUsers.Any(x => x.Id == client.CurrentUser.Id);
        var replyResult = await CheckReplyNeeded(settings, userMessage, mentioned);
        var autoReplyResult = await CheckAutomaticReplyNeeded(settings, userMessage);
        //if both failed
        if (!replyResult && !autoReplyResult && !mentioned) {
            AddRewindMessage(settings, userMessage);
        }
        //saving changes
        await context.SaveChangesAsync();
    }

    private static async Task<bool> CheckAutomaticReplyNeeded(RewindSettings settings, IUserMessage message) {
        if (settings.EnableAutomaticReplies && settings.CooldownCounter >= settings.AutomaticRepliesCooldown) {
            await Reply(settings, message);
            return true;
        }
        settings.CooldownCounter++;
        return false;
    }

    private static async Task<bool> CheckReplyNeeded(RewindSettings settings, IUserMessage message, bool mentioned) {
        if (!settings.EnablePingReplies || !mentioned) return false;
        await Reply(settings, message);
        return true;
    }

    private static async Task Reply(RewindSettings settings, IUserMessage message) {
        settings.CooldownCounter = 0;
        var rewindMessage = GetRewindMessage(settings);
        if (rewindMessage == null) return;
        var text = rewindMessage.Text;
        await message.ReplyAsync(text);
    }

    private static RewindMessage? GetRewindMessage(RewindSettings settings) {
        var count = settings.Messages.Count;
        if (count == 0) return null;
        var index = new Random().Next(0, count);
        var message = settings.Messages[index];
        settings.Messages.RemoveAt(index);
        return message;
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