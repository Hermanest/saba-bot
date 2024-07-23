namespace SabaBot;

public interface ILocalization {
    IReadOnlyCollection<string> Locales { get; }
    string DefaultLocale { get; }
    
    string this[string locale, string key] { get; }
}