using CsvHelper.Configuration.Attributes;

namespace cSharpScraper.Storage.Models;

public class StructuredScope
{
    [Name("identifier")]
    public string Identifier { get; set; }

    [Name("asset_type")]
    public string AssetType { get; set; }

    [Name("instruction")]
    public string Instruction { get; set; }

    [Name("eligible_for_bounty")]
    public bool EligibleForBounty { get; set; }

    [Name("eligible_for_submission")]
    public bool EligibleForSubmission { get; set; }
}