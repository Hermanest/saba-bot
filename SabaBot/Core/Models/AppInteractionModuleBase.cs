using Discord.Interactions;
using JetBrains.Annotations;
using SabaBot.Database;

namespace SabaBot;

public abstract class AppInteractionModuleBase : InteractionModuleBase {
    [UsedImplicitly]
    public ILocalization Localization { get; init; } = null!;
    
    [UsedImplicitly]
    public ApplicationContext DbContext { get; init; } = null!;

    protected async Task<string> GetLocalizedKey(string key) {
        var guildId = Context.Guild?.Id;
        var locale = Localization.DefaultLocale;
        if (guildId != null) {
            var settings = await GetSettingsAsync();
            locale = settings.Locale;
        }
        return Localization[locale, key];
    }

    protected async Task ModifyAsync(string? text = null) {
        await ModifyOriginalResponseAsync(x => x.Content = text);
    }
    
    protected Task<GuildSettings> GetSettingsAsync() {
        return DbContext.EnsureSettingsCreated(Context.Guild.Id);
    }
    
    protected Task SaveSettingsAsync() {
        return DbContext.SaveChangesAsync();
    }
}