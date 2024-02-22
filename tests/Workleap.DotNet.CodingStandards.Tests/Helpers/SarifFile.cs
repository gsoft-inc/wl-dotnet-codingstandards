using System.Text.Json.Serialization;

namespace Workleap.DotNet.CodingStandards.Tests.Helpers;

internal sealed class SarifFile
{
    [JsonPropertyName("runs")]
    public SarifFileRun[] Runs { get; set; }

    public IEnumerable<SarifFileRunResult> AllResults() => Runs.SelectMany(r => r.Results);

    public bool HasError() => AllResults().Any(r => r.Level == "error");
    public bool HasError(string ruleId) => AllResults().Any(r => r.Level == "error" && r.RuleId == ruleId);
    public bool HasWarning(string ruleId) => AllResults().Any(r => r.Level == "warning" && r.RuleId == ruleId);
    public bool HasNote(string ruleId) => AllResults().Any(r => r.Level == "note" && r.RuleId == ruleId);
}
