using System.Text.Json.Serialization;

namespace Workleap.DotNet.CodingStandards.Tests.Helpers;

internal sealed class SarifFileRun
{
    [JsonPropertyName("results")]
    public SarifFileRunResult[]? Results { get; set; }
}
