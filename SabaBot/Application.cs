using Newtonsoft.Json;
using Zenject;

namespace SabaBot;

internal static class Application {
    private const string DefaultConfigPath = "config.json";

    private static readonly ApplicationConfig sampleConfig = new(
        "your-token-here",
        "your-db-address-here",
        "your-llama-address-here",
        "your-llama-model-here",
        "your-llama-prompt-here"
    );

    private static readonly DiContainer applicationContainer = new();

    private static async Task Main(string[] args) {
        //loading config
        var path = args.Length > 0 ? args[0] : DefaultConfigPath;
        if (!TryLoadConfig(path, out var config)) return;
        //installing
        applicationContainer.Bind<ApplicationConfig>().FromInstance(config!).AsSingle();
        applicationContainer.Install<ApplicationInstaller>();
        await Task.Delay(-1);
    }

    private static bool TryLoadConfig(string path, out ApplicationConfig? config) {
        config = null;
        if (!File.Exists(path)) {
            Console.WriteLine("Unable to locate config.");
            WriteDefaultConfig(path);
            Console.WriteLine($"Created default config at {path}.");
            return false;
        }
        try {
            var json = File.ReadAllText(path);
            config = JsonConvert.DeserializeObject<ApplicationConfig>(json);
            return true;
        } catch (Exception ex) {
            Console.WriteLine($"Unable to load config:\n{ex}");
            return false;
        }
    }

    private static void WriteDefaultConfig(string path) {
        var json = JsonConvert.SerializeObject(sampleConfig);
        File.WriteAllText(path, json);
    }
}