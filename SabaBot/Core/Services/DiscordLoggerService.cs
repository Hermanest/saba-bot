using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace SabaBot;

internal class DiscordLoggerService(DiscordSocketClient client, ILogger logger) : IService {
    public void Start() {
        client.Log += HandleLogMessageReceived;
    }
    
    public void Dispose() {
        client.Log -= HandleLogMessageReceived;
    }

    private Task HandleLogMessageReceived(LogMessage arg) {
        var logLevel = ConvertLogSeverity(arg.Severity);
        logger.Log(logLevel, arg.Exception, arg.Message);
        return Task.CompletedTask;
    }

    private static LogLevel ConvertLogSeverity(LogSeverity severity) {
        return severity switch {
            LogSeverity.Info => LogLevel.Information,
            LogSeverity.Verbose => LogLevel.Trace,
            LogSeverity.Debug => LogLevel.Debug,
            LogSeverity.Warning => LogLevel.Warning,
            LogSeverity.Critical => LogLevel.Critical,
            LogSeverity.Error => LogLevel.Error,
            _ => throw new ArgumentOutOfRangeException(nameof(severity), severity, null)
        };
    }
}