using Discord;
using Discord.WebSocket;
using SabaBot.Database;
using Zenject;

namespace SabaBot;

internal class Bootstrapper : IDisposable, IAsyncDisposable {
    private DiscordSocketClient? _client;

    [Inject]
    public async void Start(
        DiscordSocketClient client,
        ApplicationConfig config,
        ApplicationContext context,
        [InjectOptional] IEnumerable<IService>? services
    ) {
        _client = client;
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

    public void Dispose() {
        _client?.Dispose();
    }

    public async ValueTask DisposeAsync() {
        if (_client != null) await _client.DisposeAsync();
    }
}