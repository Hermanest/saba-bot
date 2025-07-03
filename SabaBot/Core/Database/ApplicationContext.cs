using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace SabaBot.Database;

public class ApplicationContext : DbContext {
    public ApplicationContext(ApplicationConfig config, ILoggerFactory? loggerFactory = null) {
        _config = config;
        _loggerFactory = loggerFactory;
    }

    [UsedImplicitly]
    public ApplicationContext() {
        // do not remove, this constructor is used for migrations
        _migration = true;
    }

    public required DbSet<GuildSettings> Guilds { get; set; }

    private readonly ApplicationConfig? _config;
    private readonly ILoggerFactory? _loggerFactory;
    private bool _migration;

    public async Task Modify(ulong guildId, Action<GuildSettings> action) {
        var settings = await EnsureSettingsCreated(guildId);
        action(settings);
        await SaveChangesAsync();
    }

    public async Task<GuildSettings> EnsureSettingsCreated(ulong guildId) {
        var guild = await Guilds.FindAsync(guildId);
        if (guild == null) {
            guild = new() { GuildId = guildId };
            await Guilds.AddAsync(guild);
            await SaveChangesAsync();
        }
        return guild;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.Entity<GuildSettings>()
            .OwnsOne(x => x.RewindSettings)
            .OwnsMany(
                x => x.Messages,
                x => x.WithOwner()
            );
        modelBuilder.Entity<GuildSettings>()
            .OwnsOne(x => x.RewardSettings);
        modelBuilder.Entity<GuildSettings>()
            .OwnsOne(x => x.LeaveNotifSettings);
        modelBuilder.Entity<GuildSettings>()
            .OwnsOne(x => x.ReactionChampSettings)
            .OwnsMany(
                x => x.DeletedMessages,
                x => x.WithOwner()
            );
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        if (!_migration) {
            optionsBuilder.UseLoggerFactory(_loggerFactory);
            optionsBuilder.UseSqlite(
                $"Data Source={_config!.DbAddress}",
                o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
            );
        } else {
            optionsBuilder.UseSqlite("Data Source=/application.db");
        }
    }
}