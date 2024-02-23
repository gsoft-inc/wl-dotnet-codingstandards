using System.Text.Json.Serialization;

namespace Workleap.DotNet.CodingStandards.Tests.Helpers;

internal sealed class SarifFileRunResult
{
    [JsonPropertyName("ruleId")]
    public string? RuleId { get; set; }

    [JsonPropertyName("level")]
    public string? Level { get; set; }

    [JsonPropertyName("message")]
    public SarifFileRunResultMessage? Message { get; set; }

    public override string ToString()
    {
        return $"{this.Level}:{this.RuleId} {this.Message}";
    }
}
