using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using JetBrains.Annotations;
using SabaBot.Utils;

namespace SabaBot.Modules;

public class FunModule(DiscordSocketClient client) : AppInteractionModuleBase {
    [SlashCommand("shame", "Shows the wall of shame"), UsedImplicitly]
    private async Task HandleShameCommand() {
        await DeferAsync();
        var settings = await GetSettingsAsync();

        var messages = settings.ReactionChampSettings.DeletedMessages;
        if (messages.Count == 0) {
            await ModifyOriginalResponseAsync(x => x.Content = "There is no one to shame.");
            return;
        }

        // Selecting users with most messages
        var sortedUsers = messages
            .GroupBy(x => x.AuthorId)
            .OrderByDescending(x => x.Count())
            .Take(10)
            .Select(x => x.Key);

        // Fetching users
        var tasks = sortedUsers.Select(x => Context.Client.GetUserAsync(x)).ToArray();
        await Task.WhenAll(tasks);

        var first = tasks.First().Result;
        var embed = new EmbedBuilder()
            .WithAuthor(FormatUser(1, first), first.GetAvatarUrl())
            .WithDescription(
                tasks
                    .Skip(1)
                    .Zip(Enumerable.Range(2, 8))
                    .Aggregate("", (x, y) => $"{x}\n**{FormatUser(y.Second, y.First.Result)}**")
            );

        await ModifyOriginalResponseAsync(x => x.Embed = embed.Build());

        static string FormatUser(int num, IUser user) {
            return $"#{num} @{user.Username}";
        }
    }

    [SlashCommand("cringe", "Shows a random cringe message"), UsedImplicitly]
    private async Task HandleCringeCommand() {
        await DeferAsync();
        //fetching
        var settings = await GetSettingsAsync();
        var quote = settings.ReactionChampSettings.DeletedMessages.RandomOrDefault(false);
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
        await ReplyWithFormatAsync("GayMessage", user.Mention, GetRandomNumber());
    }

    [SlashCommand("dice", "Roll a dice"), UsedImplicitly]
    private async Task HandleDiceCommand() {
        await ReplyWithFormatAsync(
            "DiceMessage",
            Context.User.Mention,
            GetRandomNumber()
        );
    }

    [SlashCommand("sudoku", "Commit sudoku"), UsedImplicitly]
    private async Task HandleSudokuCommand() {
        await ReplyWithFormatAsync("SudokuMessage", Context.User.Mention);
    }

    [SlashCommand("pat", "Pat somebody"), UsedImplicitly]
    private async Task HandlePatCommand(IUser user) {
        await ReplyWithFormatAsync("PatMessage", Context.User.Mention, user.Mention);
    }

    [SlashCommand("love", "Shows how much you love the specified user."), UsedImplicitly]
    private async Task HandleLoveCommand(IUser user) {
        await ReplyWithFormatAsync(
            "LoveMessage",
            Context.User.Mention,
            user.Mention,
            GetRandomNumber()
        );
    }

    [SlashCommand("liar", "Shows the user is liar or not"), UsedImplicitly]
    private async Task HandleLiarCommand(IUser user) {
        var useAlt = GetRandomBool(1);
        var liar = GetRandomBool();
        var key = liar ? useAlt ? "AltLiarMessage" : "LiarMessage" : "NotLiarMessage";
        await ReplyWithFormatAsync(key, user.Mention);
    }

    private async Task ReplyWithFormatAsync(string messageKey, params object[] args) {
        await DeferAsync();
        var message = await GetLocalizedKey(messageKey);
        await DeleteOriginalResponseAsync();
        await ReplyAsync(string.Format(message, args));
    }

    private static bool GetRandomBool(int chance = 50) {
        return GetRandomNumber() > chance;
    }

    private static int GetRandomNumber() {
        return new Random().Next(0, 101);
    }
}