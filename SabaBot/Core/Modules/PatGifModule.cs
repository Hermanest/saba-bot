using Discord;
using Discord.Interactions;
using JetBrains.Annotations;
using ModestTree;
using SabaBot.Utils;
using SixLabors.ImageSharp;
using Zenject;
using Image = SixLabors.ImageSharp.Image;

namespace SabaBot.Modules;

public class PatGifModule([Inject] HttpClient httpClient) : InjectableInteractionModuleBase {
    [SlashCommand("patg", "Creates a pat gif of the specified user"), UsedImplicitly]
    public async Task HandlePatCommand(IUser user) {
        await DeferAsync();
        var avatar = user.GetAvatarUrl();
        var stream = await httpClient.TryOpenStreamAsync(avatar);
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
            await ModifyOriginalResponseAsync(x => x.Content = "Failed to load avatar");
        }
    }
}