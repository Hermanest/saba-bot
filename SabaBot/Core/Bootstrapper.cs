using Discord;
using Discord.WebSocket;
using SabaBot.Database;
using Zenject;

namespace SabaBot;

internal class Bootstrapper {
    [Inject]
    public async void Start(
        DiscordSocketClient client,
        ApplicationConfig config,
        ApplicationContext context,
        [InjectOptional] IEnumerable<ISystemService>? systemServices,
        [InjectOptional] IEnumerable<IService>? services
    ) {
        //bootstrapping system services
        if (systemServices != null) {
            try {
                // system services are required so if at least one
                // has failed to load we are stopping the whole application
                BootstrapServices(systemServices);
            } catch (Exception ex) {
                Application.Terminate(ex);
                return;
            }
        }
        await client.LoginAsync(TokenType.Bot, config.Token);
        await client.StartAsync();
        //bootstrapping services
        if (services != null) {
            BootstrapServices(services);
        }
    }

    private void BootstrapServices(IEnumerable<IService> services) {
        foreach (var service in services) {
            service.Start();
        }
    }
}