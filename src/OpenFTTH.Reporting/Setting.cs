using System.Text.Json.Serialization;

namespace OpenFTTH.Reporting;

internal sealed record Setting
{
    [JsonPropertyName("clientId")]
    public required string ClientId { get; init; }

    [JsonPropertyName("clientSecret")]
    public required string ClientSecret { get; init; }

    [JsonPropertyName("endPointGraphQL")]
    public required string EndPointGraphQL { get; init; }

    [JsonPropertyName("tokenEndpoint")]
    public required string TokenEndpoint { get; init; }

}
