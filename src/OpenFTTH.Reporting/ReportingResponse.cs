using System.Text.Json.Serialization;

namespace OpenFTTH.Reporting;

internal sealed record ReportingResponse
{
    [JsonPropertyName("reporting")]
    public required Reporting Reporting { get; init; }
}

internal sealed record Reporting
{
    [JsonPropertyName("customerTerminationTrace")]
    public required string[] CustomerTerminationTrace { get; init; }
}
