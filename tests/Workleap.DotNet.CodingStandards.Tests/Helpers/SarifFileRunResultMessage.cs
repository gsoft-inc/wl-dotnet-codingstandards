using System.Text.Json.Serialization;

namespace Workleap.DotNet.CodingStandards.Tests.Helpers;

internal sealed class SarifFileRunResultMessage
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    public override string ToString()
    {
        return this.Text ?? "";
    }
}
