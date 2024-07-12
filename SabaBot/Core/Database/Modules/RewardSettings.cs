using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace SabaBot.Database;

[Owned]
public class RewardSettings {
    [MaxLength(2000)] 
    public string RewardMessageText { get; set; } = "Hey. Take your reward!";

    [MaxLength(5)]
    public string? RewardClanTag { get; set; }
}