using Discord;
using Discord.Interactions;
using JetBrains.Annotations;
using SabaBot.Database;

namespace SabaBot.Modules;

[Group("rewind", "Group related to the rewind command.")]
public class RewindModule : InteractionModuleBase {
    //
    [DefaultMemberPermissions(GuildPermission.Administrator)]
    [Group("setup", "Allows you to setup the rewind command group.")]
    public class SetupModule(ApplicationContext context) : InteractionModuleBase {
        //
        [SlashCommand("auto-replies", "Allows you to setup the auto-reply feature."), UsedImplicitly]
        private async Task HandleRewindAutoRepliesSetupCommand(
            bool enable,
            int? repliesCooldown,
            int? messagesThreshold
        ) {
            await DeferAsync(true);
            var guildSettings = await context.EnsureSettingsCreated(Context.Guild.Id);
            var rewindSettings = guildSettings.RewindSettings;
            //applying settings
            if (messagesThreshold.HasValue) {
                rewindSettings.MessagesThreshold = messagesThreshold.Value;
            }
            if (repliesCooldown.HasValue) {
                rewindSettings.AutomaticRepliesCooldown = repliesCooldown.Value;
            }
            rewindSettings.EnableAutomaticReplies = enable;
            //saving
            await context.SaveChangesAsync();
            await ModifyOriginalResponseAsync(x => x.Content = "Changes saved.");
        }

        [SlashCommand("ping-replies", "Allows you to setup the ping reply feature."), UsedImplicitly]
        private async Task HandleRewindSetupCommand(bool enable) {
            await DeferAsync(true);
            var guildSettings = await context.EnsureSettingsCreated(Context.Guild.Id);
            var rewindSettings = guildSettings.RewindSettings;
            //applying settings
            rewindSettings.EnablePingReplies = enable;
            //saving
            await context.SaveChangesAsync();
            await ModifyOriginalResponseAsync(x => x.Content = "Changes saved.");
        }
    }
}