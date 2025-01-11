using Discord;
using Discord.Interactions;
using JetBrains.Annotations;
using SabaBot.Utils;

namespace SabaBot.Modules;

[Group("reaction-champ", "Group related to the reaction champ function.")]
public class ReactionChampModule(ReactionChampService champService) : AppInteractionModuleBase {
    [DefaultMemberPermissions(GuildPermission.ManageGuild)]
    [Group("setup", "Group related to the reaction champ configuration.")]
    public class SetupModule : AppInteractionModuleBase {
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

    [DefaultMemberPermissions(GuildPermission.Administrator)]
    [MessageCommand("Add to cringe"), UsedImplicitly]
    public async Task HandleCringe(IMessage msg) {
        await DeferAsync(ephemeral: true);
        
        if (msg is not IUserMessage message) {
            await ModifyAsync("Message must belong to a user.");
            return;
        }
        
        var error = await champService.AddMessage(message);
        if (error != null) {
            await ModifyAsync(error);
        } else {
            await ModifyAsync("Done!");
        }
    }
}