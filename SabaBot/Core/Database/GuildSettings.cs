using System.ComponentModel.DataAnnotations;

namespace SabaBot.Database;

public class GuildSettings {
    [Key]
    public required ulong GuildId { get; set; }

    [MaxLength(2)]
    public string Locale { get; set; } = "en";
    
    public RewindSettings RewindSettings { get; set; } = new();
    public RewardSettings RewardSettings { get; set; } = new();
}