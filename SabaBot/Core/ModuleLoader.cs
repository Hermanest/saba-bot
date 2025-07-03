using System.Reflection;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace SabaBot;

internal class ModuleLoader {
    public ModuleLoader(
        DiscordSocketClient client,
        InteractionService interactionService,
        IServiceProvider serviceProvider,
        ILogger? logger = null
    ) {
        _client = client;
        _interactionService = interactionService;
        _serviceProvider = serviceProvider;
        _logger = logger;
        _client.Ready += HandleClientReady;
    }

    private readonly DiscordSocketClient _client;
    private readonly InteractionService _interactionService;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger? _logger;
    
    private async Task HandleClientReady() {
        _client.Ready -= HandleClientReady;
        _logger?.LogInformation("Loading modules...");
        var assembly = Assembly.GetExecutingAssembly();
        var modules = await _interactionService.AddModulesAsync(assembly, _serviceProvider);
        await _interactionService.RegisterCommandsGloballyAsync();
        //printing loaded modules
        if (_logger == null) return;
        foreach (var module in modules) {
            _logger.LogInformation($"Loaded module: {module.Name}");
        }
    }
}