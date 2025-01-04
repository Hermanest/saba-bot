using Discord;
using Discord.Interactions;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using SabaBot.Database;
using SabaBot.Models.BeatLeader;
using SabaBot.Utils;

namespace SabaBot.Modules;

[Group("reward", "Group related to the reward command.")]
public class RewardModule(
    ApplicationContext context,
    IBeatLeaderAPI beatLeaderAPI
) : AppInteractionModuleBase {
    //
    [DefaultMemberPermissions(GuildPermission.ManageGuild)]
    [Group("setup", "A group responsible for the reward configuration.")]
    public class Config(
        ApplicationContext context,
        HttpClient httpClient,
        ILogger? logger = null
    ) : InteractionModuleBase {
        //
        [SlashCommand("clan-tag", "Allows you to setup a clan tag."), UsedImplicitly]
        private async Task HandleTagCommand(string? clanTag = null) {
            await DeferAsync(true);

            await context.Modify(
                Context.Guild.Id,
                x => {
                    x.RewardSettings.ClanTag = clanTag;
                }
            );

            await ModifyOriginalResponseAsync(
                x => {
                    x.Content = "Clan tag set!";
                }
            );
        }

        [SlashCommand("message", "Allows you to setup a message."), UsedImplicitly]
        private async Task HandleMessageCommand(string? message = null) {
            await DeferAsync(true);

            await context.Modify(
                Context.Guild.Id,
                x => {
                    x.RewardSettings.MessageText = message;
                }
            );

            await ModifyOriginalResponseAsync(
                x => {
                    x.Content = "Message set!";
                }
            );
        }

        [SlashCommand("attachments", "Allows you to setup attachments. Up to 3 is allowed."), UsedImplicitly]
        private async Task HandleAttachmentsCommand(
            IAttachment? attachment1 = null,
            IAttachment? attachment2 = null,
            IAttachment? attachment3 = null
        ) {
            await DeferAsync(true);

            var attachments = (Span<IAttachment?>) [attachment1, attachment2, attachment3];
            var tasks = new Task<string?>[attachments.Length];
            var filePaths = new string?[attachments.Length];

            for (var i = 0; i < attachments.Length; i++) {
                if (attachments[i] is not { } attachment) {
                    tasks[i] = Task.FromResult<string?>(null);
                    continue;
                }
                tasks[i] = WriteAttachment(attachment);
                filePaths[i] = attachment.Filename;
            }

            await Task.WhenAll(tasks);
            var errors = tasks
                .Select(x => x.Result)
                .Where(x => x != null)
                .ToArray();

            if (errors.Length > 0) {
                var text = string.Join(Environment.NewLine, errors);
                await ModifyOriginalResponseAsync(x => x.Content = $"Failed to load files!\n{text}");
                return;
            }

            await context.Modify(
                Context.Guild.Id,
                x => {
                    x.RewardSettings.FilePaths = filePaths.OfType<string>().ToArray();
                }
            );

            await ModifyOriginalResponseAsync(x => x.Content = "Attachments set!");
        }

        private async Task<string?> WriteAttachment(IAttachment attachment) {
            using var stream = await httpClient.TryOpenStreamAsync(attachment.Url, logger);
            if (stream is null) {
                return "Failed to open a stream.";
            }
            var path = $"Rewards/{attachment.Filename}";
            var error = await Resources.WriteManagedResourceAsync(Context.Guild.Id, path, stream);
            return error;
        }
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
        var clanTag = settings.ClanTag;
        if (clanTag.IsNullOrEmpty()) {
            var message = settings.SetUp ?
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
        if (settings.SetUp) {
            if (settings.FilePaths == null) {
                await Context.User.SendMessageAsync(settings.MessageText);
            } else {
                var size = settings.FilePaths.Length;
                var attachments = new FileAttachment[size];

                await ModifyAsync("Packing rewards...");

                for (var i = 0; i < size; i++) {
                    var path = "Rewards/" + settings.FilePaths[i];
                    var stream = Resources.ReadManagedResource(Context.Guild.Id, path);
                    var fileName = Path.GetFileName(settings.FilePaths[i]);
                    attachments[i] = new FileAttachment(stream, fileName);
                }

                await ModifyAsync("Sending rewards...");
                await Context.User.SendFilesAsync(attachments, settings.MessageText);
            }
            await ModifyAsync("Reward sent!");
        } else {
            await ModifyAsync("There is no reward yet!");
        }
    }
}