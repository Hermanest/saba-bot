using Discord;
using Discord.Interactions;
using JetBrains.Annotations;
using SabaBot.Database;
using SabaBot.Models.BeatLeader;
using SabaBot.Utils;

namespace SabaBot.Modules;

[Group("reward", "Group related to the reward command.")]
public class RewardModule(
    ApplicationContext context,
    HttpClient httpClient,
    IBeatLeaderAPI beatLeaderAPI
) : InteractionModuleBase {
    //
    [DefaultMemberPermissions(GuildPermission.Administrator)]
    [MessageCommand("Reward Template"), UsedImplicitly]
    private async Task HandleSetTemplateCommand(IMessage message) {
        await DeferAsync(true);
        var settings = (await context.EnsureSettingsCreated(Context.Guild.Id)).RewardSettings;
        // saving settings
        settings.MessageTemplate = message.ToSerializableMessage();
        await context.SaveChangesAsync();
        // responding
        var content = "Message template set! Please note that files on deleted messages become disposable, so this message must be persistent.";
        await ModifyOriginalResponseAsync(x => x.Content = content);
    }

    [DefaultMemberPermissions(GuildPermission.Administrator)]
    [SlashCommand("clan-tag", "Allows you to setup a clan tag."), UsedImplicitly]
    private async Task HandleSetTagCommand(string? clanTag = null) {
        await DeferAsync(true);
        var settings = (await context.EnsureSettingsCreated(Context.Guild.Id)).RewardSettings;
        // saving settings
        settings.RewardClanTag = clanTag;
        await context.SaveChangesAsync();
        // responding
        await ModifyOriginalResponseAsync(x => x.Content = "Clan tag set!");
    }

    [SlashCommand("claim", "Checks your capabilities and sends rewards if you are capable of them."), UsedImplicitly]
    private async Task HandleClaimRewardCommand() {
        await DeferAsync(true);
        var discordId = Context.User.Id;
        var playerResponse = await beatLeaderAPI.GetPlayerAsync(discordId, IdProvider.Discord);
        //returning if player is not bound
        if (!playerResponse.Succeed) {
            await ModifyOriginalResponseAsync(x => x.Content = playerResponse.Reason);
            return;
        }
        //checking for settings
        var settings = (await context.EnsureSettingsCreated(Context.Guild.Id)).RewardSettings;
        var clanTag = settings.RewardClanTag;
        if (clanTag.IsNullOrEmpty()) {
            var message = settings.MessageTemplate != null ?
                "The reward is not set up! But you're almost there, just specify a clan tag." :
                "The reward is not set up! You must set a clan tag and a message template first.";
            await ModifyOriginalResponseAsync(x => x.Content = message);
            return;
        }
        //
        var player = playerResponse.Value!;
        var clanIndex = player.clans.FindIndex(x => x.tag == clanTag);
        if (clanIndex != 0) {
            var message = clanIndex == -1 ?
                $"You should join {clanTag} in order to proceed." :
                $"You should set {clanTag} your primary clan to claim the reward.";
            await ModifyOriginalResponseAsync(x => x.Content = message);
            return;
        }
        //if all checks completed
        var response = "It seems like there is no reward!";
        if (settings.MessageTemplate is { } template) {
            await Context.User.SendMessageTemplateAsync(template, httpClient);
            response = "Reward sent!";
        }
        await ModifyOriginalResponseAsync(x => x.Content = response);
    }
}