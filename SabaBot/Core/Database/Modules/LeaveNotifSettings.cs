using Microsoft.EntityFrameworkCore;

namespace SabaBot.Database;

[Owned]
public class LeaveNotifSettings {
    public bool Enabled { get; set; }
    public ulong ChannelId { get; set; }
}