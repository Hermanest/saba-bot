using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SabaBot.Database;

namespace SabaBot;

internal static class ApplicationInstaller {
    public static ServiceProvider Install(IServiceCollection services) {
        // Base dependencies
        var config = new DiscordSocketConfig {
            GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent,
            MessageCacheSize = 1000
        };

        var socketClient = new DiscordSocketClient(config);
        services.AddSingleton(socketClient);
        services.AddSingleton(socketClient.Rest);

        var interactionService = new InteractionService(socketClient);
        services.AddSingleton(interactionService);
        services.AddDbContext<ApplicationContext>();
        
        // Localization and Resources
        services.AddSingleton<Localization>();
        services.AddSingleton<Resources>();
        
        services.AddSingleton<ILocalization>(x => x.GetRequiredService<Localization>());
        services.AddSingleton<ISystemService>(x => x.GetRequiredService<Localization>());

        // Logging
        services.AddLoggingEnhanced();

        // Services
        services.AddService<InteractionManagementService>();
        services.AddService<DiscordLoggerService>();
        services.AddService<MessageService>();
        services.AddService<ReactionChampService>();
        services.AddService<LeaveNotifService>();

        // Installing apis
        services.AddSingleton<HttpClient>();
        services.AddSingleton<IBeatLeaderAPI, BeatLeaderAPI>();
        services.AddSingleton<IChatBot, MessageRewindChatBot>();

        // Starting
        services.AddSingleton<ModuleLoader>();
        services.AddSingleton<Bootstrapper>();

        // A little workaround to start non-lazy bindings
        var provider = services.BuildServiceProvider();
        provider.GetRequiredService<ModuleLoader>();
        provider.GetRequiredService<Bootstrapper>().Start();

        return provider;
    }

    private static void AddLoggingEnhanced(this IServiceCollection services) {
        var factory = LoggerFactory.Create(x => x.AddConsole().SetMinimumLevel(LogLevel.Information));
        var logger = factory.CreateLogger("Bot");
        
        services.AddSingleton(factory);
        services.AddSingleton(logger);
        services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
    }

    private static void AddService<T>(this IServiceCollection services) where T : class, IService {
        services.AddSingleton<T>();
        services.AddSingleton<IService, T>();
    }
}