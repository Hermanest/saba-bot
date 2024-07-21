using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace SabaBot.Database;

[Owned]
public class RewardSettings {
    [MaxLength(5)]
    public string? RewardClanTag { get; set; }

    public DiscordMessage? MessageTemplate { get; set; }
}