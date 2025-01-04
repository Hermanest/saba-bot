using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace SabaBot.Database;

[Owned]
public class RewardSettings {
    [MaxLength(5)]
    public string? ClanTag { get; set; }

    public string? MessageText { get; set; }
    public string[]? FilePaths { get; set; }
    
    public bool SetUp => MessageText != null || FilePaths != null;
}