using System.CommandLine;
using cSharpScraper.Features.CommandLine;

await CommandLineParser.SetupCommandLineParser().InvokeAsync(args);