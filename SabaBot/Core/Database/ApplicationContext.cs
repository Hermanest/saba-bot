﻿using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Zenject;

namespace SabaBot.Database;

public class ApplicationContext : DbContext {
    [UsedImplicitly]
    public ApplicationContext() {
        // do not remove, this constructor is used for migrations
        _migration = true;
    }

    [Inject]
    public ApplicationContext(ApplicationConfig config, [InjectOptional] ILoggerFactory? loggerFactory) {
        _config = config;
        _loggerFactory = loggerFactory;
        // ReSharper disable once VirtualMemberCallInConstructor
        Database.EnsureCreated();
    }

    public required DbSet<GuildSettings> Guilds { get; set; }

    private readonly ApplicationConfig? _config;
    private readonly ILoggerFactory? _loggerFactory;
    private bool _migration;

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
            .OwnsOne(x => x.RewardSettings)
            .OwnsOne(x => x.MessageTemplate)
            .OwnsMany(
                x => x.Attachments,
                x => x.WithOwner()
            );
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
            optionsBuilder.UseSqlite($"Data Source={_config!.DbAddress}");
        } else {
            optionsBuilder.UseSqlite("Data Source=/application.db");
        }
    }
}