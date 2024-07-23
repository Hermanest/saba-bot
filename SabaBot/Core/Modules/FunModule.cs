using Discord;
using Discord.Interactions;
using JetBrains.Annotations;

namespace SabaBot.Modules;

public class FunModule : LocalizedInteractionModuleBase {
    [SlashCommand("gay", "Shows how much you are gay"), UsedImplicitly]
    private async Task HandleGayCommand(IUser user) {
        await DeferAsync();
        var message = await GetLocalizedKey("GayMessage");
        var percent = GetRandomNumber();
        await DeleteOriginalResponseAsync();
        await ReplyAsync(string.Format(message, user.Mention, percent));
    }
    
    [SlashCommand("love", "Shows how much you love the specified user."), UsedImplicitly]
    private async Task HandleLoveCommand(IUser user) {
        await DeferAsync();
        var message = await GetLocalizedKey("LoveMessage");
        var percent = GetRandomNumber();
        await DeleteOriginalResponseAsync();
        await ReplyAsync(string.Format(message, Context.User.Mention, user.Mention, percent));
    }
    
    [SlashCommand("liar", "Shows the user is liar or not"), UsedImplicitly]
    private async Task HandleLiarCommand(IUser user) {
        await DeferAsync();
        var useAlt = GetRandomBool(1);
        var liar = GetRandomBool();
        var key = liar ? useAlt ? "AltLiarMessage" : "LiarMessage" : "NotLiarMessage";
        var message = await GetLocalizedKey(key);
        await DeleteOriginalResponseAsync();
        await ReplyAsync(string.Format(message, user.Mention));
    }
    
    private static bool GetRandomBool(int chance = 50) {
        return GetRandomNumber() > chance;
    }
    
    private static int GetRandomNumber() {
        return new Random().Next(0, 101);
    }
}