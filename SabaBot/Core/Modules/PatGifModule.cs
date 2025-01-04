using Discord;
using Discord.Interactions;
using JetBrains.Annotations;
using ModestTree;
using SabaBot.Utils;
using SixLabors.ImageSharp;
using Zenject;
using Image = SixLabors.ImageSharp.Image;

namespace SabaBot.Modules;

public class PatGifModule([Inject] HttpClient httpClient) : DiInteractionModuleBase {
    [SlashCommand("patu", "Creates a pat gif of the specified user"), UsedImplicitly]
    public async Task HandlePatUserCommand(IUser user) {
        await HandlePatCommand(user.GetAvatarUrl());
    }

    [SlashCommand("pate", "Creates a pat gif of the specified emote"), UsedImplicitly]
    public async Task HandlePatEmoteCommand(string emote) {
        if (Emote.TryParse(emote, out var em)) {
            await HandlePatCommand(em.Url);
        } else {
            await RespondAsync("Invalid emote specified!", ephemeral: true);
        }
    }

    private async Task HandlePatCommand(string imageUrl) {
        await DeferAsync();
        var stream = await httpClient.TryOpenStreamAsync(imageUrl);
        if (stream != null) {
            //making gif 
            var image = await Image.LoadAsync(stream);
            var gif = PatGifMaker.Make(image);
            await stream.DisposeAsync();
            //sending gif
            stream = new MemoryStream();
            await gif.SaveAsGifAsync(stream);
            await Context.Interaction.ModifyOriginalResponseAsync(
                x => {
                    x.Content = " ";
                    x.Attachments = new List<FileAttachment> {
                        new(stream, "pat.gif")
                    };
                }
            );
            await stream.DisposeAsync();
        } else {
            await ModifyOriginalResponseAsync(
                x => x.Content = "Failed to load the image!"
            );
        }
    }
}