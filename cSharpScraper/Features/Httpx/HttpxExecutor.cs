using System.Text.Json;

namespace cSharpScraper.Features.Httpx;

public class HttpxExecutor(ILogger<HttpxExecutor> logger) : IHttpxExecutor
{
    private readonly ILogger<HttpxExecutor> _logger = logger;

    public async Task<List<HttpxResult>> GetResponsiveUrlsAsync(IEnumerable<string> urls)
    {
        var command = "httpx -silent -follow-redirects -status-code -no-color -json";
        var (process, stdout, stderr) = await ExecuteShellCommandAsync(urls, command);

        LogProcessResult(stderr, process.ExitCode);
        
        return await ParseHttpxResult(stdout);
    }
    
    
    private void LogProcessResult(string stderr, int exitCode)
    {
        if (exitCode != 0)
        {
            _logger.LogWarning("httpx exited with code {ExitCode}", exitCode);
        }

        if (!string.IsNullOrWhiteSpace(stderr))
        {
            _logger.LogError("stderr: {Error}", stderr.Trim());
        }
    }

    private static async Task<List<HttpxResult>> ParseHttpxResult(string stdout)
    {
        var results = new List<HttpxResult>();
        using var reader = new StringReader(stdout);
        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            try
            {
                var result = JsonSerializer.Deserialize<HttpxResult>(line);
                if (result != null)
                    results.Add(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to parse line: {ex.Message}");
            }
        }
        return results;
    }

    private async Task<(Process process, string stdout, string stderr)> ExecuteShellCommandAsync(IEnumerable<string> urls, string command)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"-c \"{command}\"",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        
        process.Start();

        await process.StandardInput.WriteAsync(string.Join(Environment.NewLine, urls));
        process.StandardInput.Close();

        _logger.LogDebug("Running command: {Command}", command);
        
        var stdout = await process.StandardOutput.ReadToEndAsync();
        var stderr = await process.StandardError.ReadToEndAsync();
        
        await process.WaitForExitAsync();
        return (process, stdout, stderr);
    }
}