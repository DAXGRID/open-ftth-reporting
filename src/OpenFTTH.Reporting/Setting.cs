using System.Text.Json.Serialization;

namespace OpenFTTH.Reporting;

internal sealed record Setting
{
    [JsonPropertyName("clientId")]
    public required string ClientId { get; init; }

    [JsonPropertyName("clientSecret")]
    public required string ClientSecret { get; init; }

    [JsonPropertyName("reportApiBaseAddress")]
    public required string ReportApiBaseAddress { get; init; }

    [JsonPropertyName("tokenEndpoint")]
    public required string TokenEndpoint { get; init; }

    [JsonPropertyName("uploadPath")]
    public required string UploadPath { get; init; }

    [JsonPropertyName("fileServer")]
    public required FileServerSetting FileServer { get; init; }
}

internal sealed record FileServerSetting
{
    [JsonPropertyName("hostAddress")]
    public required string HostAddress { get; init; }

    [JsonPropertyName("username")]
    public required string Username { get; init; }

    [JsonPropertyName("password")]
    public required string Password { get; init; }
}
