using Discord;
using Discord.Interactions;
using JetBrains.Annotations;
using SabaBot.Utils;

namespace SabaBot.Modules;

[DefaultMemberPermissions(GuildPermission.Administrator)]
[Group("reaction-champ", "Group related to the reaction champ function.")]
public class ReactionChampModule : LocalizedInteractionModuleBase {
    //
    [SlashCommand("emote", "Sets the removal emote."), UsedImplicitly]
    private async Task HandleSetEmoteCommand(string emote) {
        if (!DiscordUtils.TryParseEmote(emote, out _)) {
            await RespondAsync(ephemeral: true, embed: EmbedUtils.BuildErrorEmbed("Invalid emote specified."));
            return;
        }
        await DeferAsync(ephemeral: true);
        //saving
        var settings = await GetSettingsAsync();
        settings.ReactionChampSettings.EmoteId = emote;
        await SaveSettingsAsync();
        //replying
        await RespondOk(
            ("Emote", emote)
        );
    }

    [SlashCommand("enabled", "Sets is the module enabled or not."), UsedImplicitly]
    private async Task HandleEnabledCommand(bool enable) {
        await DeferAsync(ephemeral: true);
        //saving
        var settings = await GetSettingsAsync();
        settings.ReactionChampSettings.Enabled = enable;
        await SaveSettingsAsync();
        //replying
        await RespondOk(
            ("Enabled", enable.ToString())
        );
    }

    [SlashCommand("threshold", "Sets the removal threshold."), UsedImplicitly]
    private async Task HandleThresholdCommand(int threshold) {
        await DeferAsync(ephemeral: true);
        //saving
        var settings = await GetSettingsAsync();
        settings.ReactionChampSettings.ReactionThreshold = threshold;
        await SaveSettingsAsync();
        //replying
        await RespondOk(
            ("Threshold", threshold.ToString())
        );
    }

    private async Task RespondOk(params (string, string)[] changedValues) {
        var embed = EmbedUtils.BuildReportEmbed(changedValues);
        await ModifyOriginalResponseAsync(x => x.Embed = embed);
    }
}