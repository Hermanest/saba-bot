using Discord;
using Discord.Interactions;
using JetBrains.Annotations;
using SabaBot.Database;
using SabaBot.Models.BeatLeader;
using SabaBot.Utils;

namespace SabaBot.Modules;

[Group("reward", "Group related to the reward command.")]
public class RewardModule(ApplicationContext context, IBeatLeaderAPI beatLeaderAPI) : InteractionModuleBase {
    //
    [DefaultMemberPermissions(GuildPermission.Administrator)]
    [SlashCommand("set", "Allows you to setup the reward command group."), UsedImplicitly]
    private async Task HandleSetRewardCommand(string rewardText, string? clanTag = null) {
        await DeferAsync(true);
        var settings = (await context.EnsureSettingsCreated(Context.Guild.Id)).RewardSettings;
        settings.RewardMessageText = rewardText;
        settings.RewardClanTag = clanTag;
        await context.SaveChangesAsync();
        await ModifyOriginalResponseAsync(x => x.Content = "Values set!");
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
            await ModifyOriginalResponseAsync(x => x.Content = "The reward is not set up.");
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
        await Context.User.SendMessageAsync(settings.RewardMessageText);
        await ModifyOriginalResponseAsync(x => x.Content = "Reward sent!");
    }
}