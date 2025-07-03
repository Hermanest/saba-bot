using Discord;
using Discord.WebSocket;

namespace SabaBot;

internal class Bootstrapper(
    DiscordSocketClient client,
    ApplicationConfig config,
    IEnumerable<ISystemService>? systemServices = null,
    IEnumerable<IService>? services = null
) {
    public async void Start() {
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

    private static void BootstrapServices(IEnumerable<IService> services) {
        foreach (var service in services) {
            service.Start();
        }
    }
}