using Microsoft.EntityFrameworkCore;

namespace SabaBot.Database;

[Owned]
public class DiscordMessage {
    public required string Content { get; set; }
    public List<DiscordAttachment> Attachments { get; set; } = new();
}