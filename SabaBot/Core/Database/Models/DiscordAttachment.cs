using System.ComponentModel.DataAnnotations;

namespace SabaBot.Database;

public class DiscordAttachment {
    [Key]
    public int Id { get; set; }
    public required string FileUrl { get; set; }
    public required string FileName { get; set; }
}