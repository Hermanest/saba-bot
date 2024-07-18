using System.Diagnostics;
using System.Text.RegularExpressions;
using Discord;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SabaBot.Database;
using SabaBot.Utils;
using Zenject;

namespace SabaBot;

#pragma warning disable SKEXP0010

internal class OpenAIChatBot(
    ApplicationConfig config,
    [InjectOptional] ILogger? logger = null,
    [InjectOptional] ILoggerFactory? loggerFactory = null
) : IChatBot {
    private IChatCompletionService? _chatCompletionService;

    [Inject]
    public void Initialize() {
        if (!Uri.TryCreate(config.LlamaAddress, UriKind.RelativeOrAbsolute, out var uri)) {
            logger?.LogError("Invalid LlamaAddress is specified. AI ChatBot won't be available!");
            return;
        }
        _chatCompletionService = new OpenAIChatCompletionService(
            modelId: config.LlamaModelId,
            endpoint: uri,
            loggerFactory: loggerFactory
        );
    }

    public void Dispose() { }

    public async Task ReplyAsync(RewindSettings settings, IUserMessage message) {
        if (_chatCompletionService == null) {
            await message.ReplyAsync("AI ChatBot is not set up.");
            return;
        }
        var stopwatch = new Stopwatch();
        //
        var prompt = CreatePrompt(message);
        logger?.LogDebug($"Requesting with prompt: {prompt.Last()}");
        //
        stopwatch.Start();
        var responseEnumerable = _chatCompletionService.GetStreamingChatMessageContentsAsync(prompt);
        var response = await responseEnumerable.AggregateAsync("", (current, line) => current + line.Content);
        logger?.LogInformation($"Generation took {stopwatch.Elapsed}. Responding.");
        stopwatch.Stop();
        //
        await message.ReplyAsync(response);
    }

    private ChatHistory CreatePrompt(IUserMessage message) {
        //system prompt
        var definedPrompt = config.LlamaPrompt;
        var systemPrompt = $"Use this info to properly form your answer: {definedPrompt}";
        systemPrompt += $"Sender name is {message.Author.Username}";
        //use prompt
        var userPrompt = CleanupPrompt(message.Content);
        //making chat history
        var chatHistory = new ChatHistory();
        chatHistory.AddMessage(AuthorRole.System, systemPrompt);
        //adding user prompt if valid
        var referencedMessage = message.ReferencedMessage;
        if (referencedMessage != null) {
            var referencedPrompt = CleanupPrompt(referencedMessage.Content);
            chatHistory.AddMessage(AuthorRole.Assistant, referencedPrompt);
        }
        if (!userPrompt.IsNullOrEmpty()) {
            chatHistory.AddMessage(AuthorRole.User, userPrompt);
        }
        //
        return chatHistory;
    }

    private static string CleanupPrompt(string content) {
        return new Regex("<@[^>]*>").Replace(content, "");
    }
}