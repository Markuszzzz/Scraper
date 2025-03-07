using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;

namespace cSharpScraper.Crawler;

public class CustomConsoleFormatter : ConsoleFormatter
{
    public CustomConsoleFormatter() : base(nameof(CustomConsoleFormatter)) { }

    public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider scopeProvider, TextWriter textWriter)
    {
        string logLevelColor = logEntry.LogLevel switch
        {
            LogLevel.Trace => "\u001b[37m",   
            LogLevel.Debug => "\u001b[36m", 
            LogLevel.Information => "\u001b[32m", 
            LogLevel.Warning => "\u001b[33m", 
            LogLevel.Error => "\u001b[31m",   
            LogLevel.Critical => "\u001b[35m", 
            _ => "\u001b[0m"                 
        };

        
        const string resetColor = "\u001b[0m";

        string logLevel = logEntry.LogLevel.ToString();
        string message = logEntry.State?.ToString();

        textWriter.WriteLine($"{logLevelColor}{logLevel}: {message}{resetColor}");
    }
}