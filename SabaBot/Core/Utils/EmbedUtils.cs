using Discord;

namespace SabaBot.Utils;

internal static class EmbedUtils {
    public static Embed BuildSuccessEmbed(string info) {
        return BuildUniversalEmbed(":white_check_mark: Execution completed", info, Color.Green);
    }

    public static Embed BuildErrorEmbed(Exception exception) {
        return BuildErrorEmbed(exception.ToString(), true);
    }

    public static Embed BuildErrorEmbed(string error, bool internalError = false) {
        return BuildUniversalEmbed("❌ Execution error! " + (internalError ? "(Internal)" : string.Empty), error, Color.Red);
    }

    public static Embed BuildUniversalEmbed(string title, string info, Color color) {
        return new EmbedBuilder().WithTitle(title).WithColor(color).WithDescription(info).Build();
    }
    
    public static Embed BuildReportEmbed(params (string, string)[] changedValues) {
        var info = changedValues.Aggregate("", (x, y) => $"{x}\n{y.Item1}: {y.Item2}");
        return BuildUniversalEmbed("Modification report", info, Color.Blue);
    }
}