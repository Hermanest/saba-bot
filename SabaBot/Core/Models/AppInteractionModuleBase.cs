using SabaBot.Database;
using Zenject;

namespace SabaBot;

public abstract class AppInteractionModuleBase : DiInteractionModuleBase {
    [Inject] private readonly ILocalization _localization = null!;
    [Inject] protected readonly ApplicationContext DbContext = null!;

    protected async Task<string> GetLocalizedKey(string key) {
        var guildId = Context.Guild?.Id;
        var locale = _localization.DefaultLocale;
        if (guildId != null) {
            var settings = await GetSettingsAsync();
            locale = settings.Locale;
        }
        return _localization[locale, key];
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