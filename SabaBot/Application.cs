using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using SabaBot.Database;

namespace SabaBot;

internal static class Application {
    private const string DefaultConfigPath = "config.json";

    private static readonly ApplicationConfig sampleConfig = new(
        "your-token-here",
        "your-db-address-here",
        "your-llama-address-here",
        "your-llama-model-here",
        "your-llama-prompt-here",
        "your-resources-path"
    );

    private static readonly TaskCompletionSource taskCompletionSource = new();

    private static async Task Main(string[] args) {
        // Loading config
        var path = args.Length > 0 ? args[0] : DefaultConfigPath;
        if (!TryLoadConfig(path, out var config)) return;

        // Installing
        var services = new ServiceCollection();
        services.AddSingleton(config!);
        var provider = ApplicationInstaller.Install(services);

        // Migrating the DB if needed
        var db = provider.GetRequiredService<ApplicationContext>();
        await db.Database.MigrateAsync();

        await taskCompletionSource.Task;
    }

    public static void Terminate(Exception ex) {
        taskCompletionSource.SetException(ex);
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

            if (config == null) {
                throw new Exception("Deserialization failure");
            }

            // Note that Path.Combine takes into account rooted paths, so:
            // /home + /config.json = /config.json
            // /home + config.json = /home/config.json

            var configDir = Path.GetDirectoryName(path) ?? string.Empty;
            var dbAddress = config.DbAddress;

            {
                var dbFile = Path.Combine(configDir, dbAddress);

                // DB can be either a path or a network address, so we check if the file exists first
                if (File.Exists(dbFile)) {
                    dbAddress = dbFile;
                }
            }

            config = config with {
                DbAddress = dbAddress,
                LocalizationFile = Path.Combine(configDir, config.LocalizationFile),
                ResourcesPath = Path.Combine(configDir, config.ResourcesPath)
            };

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