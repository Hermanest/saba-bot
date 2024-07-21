using Sini;

namespace SabaBot;

internal class Localization(ApplicationConfig config) : ISystemService, ILocalization {
    private static readonly string[] localizationKeys = [
        "LlamaPrompt"
    ];

    public IReadOnlyCollection<string> Locales => _locales.Keys;

    public string this[string locale, string key] => Get(locale, key);

    private readonly Dictionary<string, Dictionary<string, string>> _locales = new();
    private readonly List<string> _buffer = new(localizationKeys.Length);

    public void Start() {
        var file = new IniFile(config.LocalizationFile);
        if (file.Sections.Length == 0) {
            throw new InvalidDataException("Localization must contain at least one locale");
        }
        foreach (var section in file.Sections) {
            AddLocale(section, file);
        }
    }

    private void AddLocale(string section, IniFile file) {
        _buffer.Clear();
        _buffer.AddRange(localizationKeys);
        var locale = new Dictionary<string, string>();
        foreach (var key in file.GetKeys(section)) {
            var value = file.GetStr(section, key);
            if (value == null) continue;
            locale.Add(key, value);
            _buffer.Remove(key);
        }
        if (_buffer.Count > 0) {
            var missing = _buffer.Aggregate("", (acc, item) => $"{acc}, {item}");
            throw new InvalidDataException($"Locale {section} is missing these keys: {missing}");
        }
        _locales.Add(section, locale);
    }

    public string Get(string locale, string key) {
        return _locales[locale][key];
    }
}