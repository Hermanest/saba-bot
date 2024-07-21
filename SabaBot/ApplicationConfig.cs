namespace SabaBot;

public record ApplicationConfig(
    string Token,
    string DbAddress,
    string LlamaAddress,
    string LlamaModelId,
    string LocalizationFile
);