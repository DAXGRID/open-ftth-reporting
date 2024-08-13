using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenFTTH.Reporting;

internal static class Auth
{
    public static async Task<JsonWebToken> GetTokenAsync(
        HttpClient httpClient,
        Uri tokenEndPoint,
        string clientId,
        string clientSecret)
    {
       var credentials = new Dictionary<string, string>
        {
            { "grant_type", "client_credentials" },
            { "client_id", clientId },
            { "client_secret", clientSecret },
        };

        using var content = new FormUrlEncodedContent(credentials);

        var response = await httpClient
            .PostAsync(tokenEndPoint, content)
            .ConfigureAwait(false);

        var tokenResponseBody = await response.Content
            .ReadAsStringAsync()
            .ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Could not receive token, received statuscode: '{response.StatusCode}'. Response body '{tokenResponseBody}'.");
        }

        return JsonSerializer.Deserialize<JsonWebToken>(tokenResponseBody) ??
            throw new InvalidOperationException("Could not deserialize the JWT.");
    }
}

internal sealed record JsonWebToken
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }

    [JsonPropertyName("expires_in")]
    public int ExpiresInSeconds { get; set; }

    public DateTime ExpiresAt { get; set; }

    [JsonConstructor]
    public JsonWebToken(string accessToken, int expiresInSeconds)
    {
        if (String.IsNullOrEmpty(accessToken))
        {
            throw new ArgumentNullException(nameof(accessToken));
        }

        AccessToken = accessToken;
        ExpiresAt = DateTime.UtcNow.AddSeconds(expiresInSeconds);
    }
}
