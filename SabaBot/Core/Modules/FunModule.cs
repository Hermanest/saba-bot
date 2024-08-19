using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using JetBrains.Annotations;
using SabaBot.Utils;

namespace SabaBot.Modules;

public class FunModule(DiscordSocketClient client) : LocalizedInteractionModuleBase {
    [SlashCommand("cringe", "Shows a random cringe message"), UsedImplicitly]
    private async Task HandleCringeCommand() {
        await DeferAsync();
        //fetching
        var settings = await GetSettingsAsync();
        var quote = settings.ReactionChampSettings.DeletedMessages.RandomOrDefault();
        if (quote == null) {
            var str = await GetLocalizedKey("NoCringeMessage");
            await ModifyOriginalResponseAsync(x => x.Content = str);
            return;
        }
        await SaveSettingsAsync();
        var author = await client.GetUserAsync(quote.AuthorId);
        //making embed
        var embed = new EmbedBuilder()
            .WithAuthor(
                new EmbedAuthorBuilder()
                    .WithName(author.Username)
                    .WithIconUrl(author.GetAvatarUrl())
            )
            .WithDescription(quote.Text)
            .WithColor(Color.Blue)
            .Build();
        //modifying
        await ModifyOriginalResponseAsync(x => x.Embed = embed);
    }

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