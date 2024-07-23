using SabaBot.Database;
using Zenject;

namespace SabaBot;

public abstract class LocalizedInteractionModuleBase : InjectableInteractionModuleBase {
    [Inject] private readonly ILocalization _localization = null!;
    [Inject] protected readonly ApplicationContext DbContext = null!;

    protected async Task<string> GetLocalizedKey(string key) {
        var guildId = Context.Guild?.Id;
        var locale = _localization.DefaultLocale;
        if (guildId != null) {
            var settings = await GetSettings();
            locale = settings.Locale;
        }
        return _localization[locale, key];
    }

    protected Task<GuildSettings> GetSettings() {
        return DbContext.EnsureSettingsCreated(Context.Guild.Id);
    }
}