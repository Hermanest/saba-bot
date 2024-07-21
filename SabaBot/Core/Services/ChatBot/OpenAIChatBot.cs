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
    ApplicationContext context,
    ILocalization localization,
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

    public async Task ReplyAsync(RewindSettings settings, IUserMessage message) {
        if (_chatCompletionService == null) {
            await message.ReplyAsync("AI ChatBot is not set up.");
            return;
        }
        var stopwatch = new Stopwatch();
        //forming prompt
        var user = message.Author as IGuildUser;
        var guildSettings = await context.EnsureSettingsCreated(user!.GuildId);
        var prompt = CreatePrompt(guildSettings.Locale, message);
        logger?.LogDebug($"Requesting with prompt: {prompt.Last()}");
        //requesting
        stopwatch.Start();
        var responseEnumerable = _chatCompletionService.GetStreamingChatMessageContentsAsync(prompt);
        var response = await responseEnumerable.AggregateAsync("", (current, line) => current + line.Content);
        if (response.Length > 2000) {
            response = response[..2000];
        }
        logger?.LogInformation($"Generation took {stopwatch.Elapsed}. Responding.");
        stopwatch.Stop();
        //replying
        await message.ReplyAsync(response);
    }

    private ChatHistory CreatePrompt(string locale, IUserMessage message) {
        //system prompt
        var definedPrompt = localization[locale, "LlamaPrompt"];
        var systemPrompt = $"Use this info to properly form your answer: {definedPrompt}";
        //use prompt
        var userPrompt = CleanupPrompt(message.Content);
        //making chat history
        var chatHistory = new ChatHistory();
        chatHistory.AddMessage(AuthorRole.System, systemPrompt);
        //adding user prompt if valid
        var referencedMessage = message.ReferencedMessage;
        if (referencedMessage != null) {
            var referencedPrompt = CleanupPrompt(referencedMessage.Content);
            AddUserMessage(chatHistory, referencedPrompt, referencedMessage.Author.Username);
        }
        if (!userPrompt.IsNullOrEmpty()) {
            AddUserMessage(chatHistory, userPrompt, message.Author.Username);
        }
        //
        return chatHistory;
    }

    private static void AddUserMessage(ChatHistory history, string message, string author) {
        history.AddMessage(AuthorRole.User, $"Author: {author}\n{message}");
    }

    private static string CleanupPrompt(string content) {
        return new Regex("<@[^>]*>").Replace(content, "");
    }
}