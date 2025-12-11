using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SabaBot.Database;

namespace SabaBot;

public class LeaveNotifService(
    DiscordSocketClient client,
    IDbContextFactory<ApplicationContext> contextFactory,
    ILogger logger
) : IService, IDisposable {
    public void Dispose() {
        client.UserLeft -= HandleLeftGuild;
    }

    public void Start() {
        client.UserLeft += HandleLeftGuild;
    }

    private async Task HandleLeftGuild(SocketGuild guild, SocketUser user) {
        var context = await contextFactory.CreateDbContextAsync();
        var settings = await context.Guilds.FindAsync(guild.Id);
        
        if (settings == null || !settings.LeaveNotifSettings.Enabled) {
            return;
        }

        var channel = await client.GetChannelAsync(settings.LeaveNotifSettings.ChannelId);
        if (channel is not IMessageChannel msgChannel) {
            logger.LogDebug("User logging is enabled, but channel is not specified");
            return;
        }

        await msgChannel.SendMessageAsync(text: $"{user.Mention} left the server!");
    }
}