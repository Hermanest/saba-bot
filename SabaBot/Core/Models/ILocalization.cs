namespace SabaBot;

public interface ILocalization {
    IReadOnlyCollection<string> Locales { get; }
    
    string this[string locale, string key] { get; }
}