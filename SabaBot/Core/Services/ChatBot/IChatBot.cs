using Discord;
using SabaBot.Database;

namespace SabaBot;

public interface IChatBot {
    Task ReplyAsync(RewindSettings settings, IUserMessage message);
}