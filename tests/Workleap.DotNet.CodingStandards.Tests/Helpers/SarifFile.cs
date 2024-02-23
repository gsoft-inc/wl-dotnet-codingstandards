using System.Text.Json.Serialization;

namespace Workleap.DotNet.CodingStandards.Tests.Helpers;

internal sealed class SarifFile
{
    [JsonPropertyName("runs")]
    public SarifFileRun[]? Runs { get; set; }

    public IEnumerable<SarifFileRunResult> AllResults() => this.Runs?.SelectMany(r => r.Results ?? []) ?? [];

    public bool HasError() => this.AllResults().Any(r => r.Level == "error");
    public bool HasError(string ruleId) => this.AllResults().Any(r => r.Level == "error" && r.RuleId == ruleId);
    public bool HasWarning(string ruleId) => this.AllResults().Any(r => r.Level == "warning" && r.RuleId == ruleId);
    public bool HasNote(string ruleId) => this.AllResults().Any(r => r.Level == "note" && r.RuleId == ruleId);
}
