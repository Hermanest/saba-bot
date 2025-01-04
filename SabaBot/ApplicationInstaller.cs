using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using SabaBot.Database;
using Zenject;

namespace SabaBot;

internal class ApplicationInstaller : Installer {
    public override void InstallBindings() {
        //adapters
        Container.BindInterfacesAndSelfTo<ZenjectServiceScopeFactory>().AsSingle().Lazy();
        Container.BindInterfacesAndSelfTo<ZenjectServiceProvider>().AsSingle().Lazy();
        //base dependencies
        var config = new DiscordSocketConfig {
            GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent,
            MessageCacheSize = 1000
        };
        var socketClient = new DiscordSocketClient(config);
        Container.Bind<DiscordSocketClient>().FromInstance(socketClient).AsSingle().Lazy();
        Container.Bind<DiscordRestClient>().FromInstance(socketClient.Rest).AsSingle().Lazy();
        Container.Bind<InteractionService>().AsSingle().Lazy();
        Container.Bind<ApplicationContext>().AsSingle().Lazy();
        //logging
        InstallLogger();
        //services
        InstallServices();
        //installing apis
        InstallAPI();
        //starting
        Container.Bind<ModuleLoader>().AsSingle().NonLazy();
        Container.Bind<Bootstrapper>().AsSingle().NonLazy();
        //a little workaround to start non-lazy bindings
        Bootstrap();
    }

    private void InstallAPI() {
        Container.Bind<HttpClient>().AsSingle();
        Container.BindInterfacesTo<BeatLeaderAPI>().AsSingle();
    }

    private void InstallServices() {
        Container.Bind(typeof(ISystemService), typeof(ILocalization)).To<Localization>().AsSingle();
        Container.BindInterfacesTo<DiscordLoggerService>().AsSingle();
        Container.BindInterfacesTo<InteractionManagementService>().AsSingle();
        //Container.BindInterfacesTo<OpenAIChatBot>().AsSingle();
        Container.BindInterfacesTo<MessageRewindChatBot>().AsSingle();
        Container.BindInterfacesTo<MessageService>().AsSingle();
        Container.BindInterfacesTo<ReactionChampService>().AsSingle();
    }
    
    private void InstallLogger() {
        var factory = LoggerFactory.Create(x => x.AddConsole().SetMinimumLevel(LogLevel.Information));
        var logger = factory.CreateLogger("Bot");
        Container.Bind<ILoggerFactory>().FromInstance(factory).AsSingle();
        Container.Bind<ILogger>().FromInstance(logger).AsTransient();
    }

    private void Bootstrap() {
        Container.Resolve<ModuleLoader>();
        Container.Resolve<Bootstrapper>();
    }
}