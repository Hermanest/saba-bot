using Discord;
using Discord.Interactions;
using JetBrains.Annotations;
using SabaBot.Utils;

namespace SabaBot.Modules;

[DefaultMemberPermissions(GuildPermission.ManageGuild)]
[Group("leave-notif", "Group related to the leave notifications function.")]
public class LeaveNotifModule : AppInteractionModuleBase {
    [SlashCommand("enabled", "Sets is the module enabled or not."), UsedImplicitly]
    private async Task HandleEnabledCommand(bool enable) {
        await DeferAsync(ephemeral: true);
        //saving
        var settings = await GetSettingsAsync();
        settings.LeaveNotifSettings.Enabled = enable;
        await SaveSettingsAsync();
        //replying
        await RespondOk(
            ("Enabled", enable.ToString())
        );
    }

    [SlashCommand("channel", "Sets the channel."), UsedImplicitly]
    private async Task HandleThresholdCommand(IChannel channel) {
        await DeferAsync(ephemeral: true);
        //saving
        var settings = await GetSettingsAsync();
        settings.LeaveNotifSettings.ChannelId = channel.Id;
        await SaveSettingsAsync();
        //replying
        await RespondOk(
            ("Channel", channel.Id.ToString())
        );
    }

    private async Task RespondOk(params (string, string)[] changedValues) {
        var embed = EmbedUtils.BuildReportEmbed(changedValues);
        await ModifyOriginalResponseAsync(x => x.Embed = embed);
    }
}