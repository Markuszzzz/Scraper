using System.CommandLine;
using cSharpScraper.CommandLineParsing;

await CommandLineParser.SetupCommandLineParser().InvokeAsync(args);