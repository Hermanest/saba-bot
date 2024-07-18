using Discord;
using SabaBot.Database;

namespace SabaBot;

internal class MessageRewindChatBot : IChatBot {
    private static RewindMessage? GetRewindMessage(RewindSettings settings) {
        var count = settings.Messages.Count;
        if (count == 0) return null;
        var index = new Random().Next(0, count);
        var message = settings.Messages[index];
        settings.Messages.RemoveAt(index);
        return message;
    }

    public async Task ReplyAsync(RewindSettings settings, IUserMessage message) {
        var rewindMessage = GetRewindMessage(settings);
        if (rewindMessage == null) return;
        var text = rewindMessage.Text;
        await message.ReplyAsync(text);
    }
}