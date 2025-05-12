using System.Text.RegularExpressions;

namespace cSharpScraper.Features.SubDomainCrawler;

public class SubdomainDiscoveryService(ILogger<SubdomainDiscoveryService> logger)
    : ISubdomainDiscoveryService
{
    private readonly ILogger<SubdomainDiscoveryService> _logger = logger;

    public async Task<List<string>> DiscoverAsync(string registrableDomain)
    {
        var command = $"subfinder -d {registrableDomain} -silent | httpx -follow-redirects -silent -no-color";
        _logger.LogDebug("Running command: {Command}", command);

        var (output, error, exitCode) = await ExecuteShellCommandAsync(command);

        LogProcessResult(error, exitCode, registrableDomain);

        return ParseSubdomains(output);
    }
    
    private static async Task<(string stdout, string stderr, int exitCode)> ExecuteShellCommandAsync(string command)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"-c \"{command}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();

        var stdout = await process.StandardOutput.ReadToEndAsync();
        var stderr = await process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        return (stdout, stderr, process.ExitCode);
    }

    private void LogProcessResult(string stderr, int exitCode, string domain)
    {
        if (exitCode != 0)
        {
            _logger.LogWarning("Process exited with code {ExitCode} for domain {Domain}", exitCode, domain);
        }

        if (!string.IsNullOrWhiteSpace(stderr))
        {
            _logger.LogError("stderr: {Error}", stderr.Trim());
        }
    }

    private List<string> ParseSubdomains(string output)
    {
        var lines = output.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        var subdomains = new List<string>();

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            var redirectMatch = Regex.Match(trimmed, @"\[(.*?)\]");

            if (redirectMatch.Success)
            {
                var redirectedUrl = redirectMatch.Groups[1].Value;
                _logger.LogDebug("Detected redirected URL: {RedirectedUrl}", redirectedUrl);
                subdomains.Add(redirectedUrl);
            }
            else
            {
                _logger.LogDebug("Detected URL: {Url}", trimmed);
                subdomains.Add(trimmed);
            }
        }

        return subdomains.Distinct().ToList();
    }
}