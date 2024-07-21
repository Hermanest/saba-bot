using Discord;
using SabaBot.Database;

namespace SabaBot.Utils;

internal static class DiscordUtils {
    public static DiscordMessage ToSerializableMessage(this IMessage message) {
        return new DiscordMessage {
            Content = message.Content,
            Attachments = message.Attachments.Select(
                x => new DiscordAttachment {
                    FileUrl = x.Url,
                    FileName = x.Filename
                }
            ).ToList()
        };
    }

    public static async Task SendMessageTemplateAsync(this IUser user, DiscordMessage template, HttpClient client) {
        var attachments = template.Attachments.Select(attachment => TryCreateAttachment(attachment, client)).ToArray();
        await Task.WhenAll(attachments);
        var files = new List<FileAttachment>();
        var failedQuery = "";
        foreach (var task in attachments) {
            var (name, file) = task.Result;
            if (file == null) {
                failedQuery += $"*[{name} has failed to load]*";
            } else {
                files.Add(file.Value);
            }
        }
        var message = $"{template.Content}\n{failedQuery}";
        await user.SendFilesAsync(files, message);
    }

    private static async Task<(string, FileAttachment?)> TryCreateAttachment(DiscordAttachment att, HttpClient client) {
        var stream = await client.TryOpenStreamAsync(att.FileUrl);
        return (att.FileName, stream == null ? null : new FileAttachment(stream, att.FileName));
    }
}