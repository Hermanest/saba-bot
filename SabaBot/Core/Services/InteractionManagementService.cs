using Discord.Interactions;
using Discord.WebSocket;

namespace SabaBot;

internal class InteractionManagementService(
    DiscordSocketClient client,
    InteractionService interactionService,
    IServiceProvider serviceProvider
) : IService {
    public void Start() {
        client.InteractionCreated += HandleInteractionCreated;
    }

    public void Dispose() {
        client.InteractionCreated -= HandleInteractionCreated;
    }

    private async Task HandleInteractionCreated(SocketInteraction interaction) {
        await interactionService.ExecuteCommandAsync(
            new SocketInteractionContext(client, interaction),
            serviceProvider
        );
    }
}