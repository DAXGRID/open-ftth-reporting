using GraphQL;
using Microsoft.Extensions.Logging;

namespace OpenFTTH.Reporting;

internal sealed class StartUp
{
    private readonly ILogger _logger;
    private readonly Setting _setting;

    public StartUp(ILogger logger, Setting setting)
    {
        _logger = logger;
        _setting = setting;
    }

    public async Task StartAsync()
    {
        using var httpClient = new HttpClient()
        {
            BaseAddress = new Uri(_setting.EndPointGraphQL)
        };

        using var authGraphQLClient = new AuthGraphQLClient(
            httpClient,
            new(_setting.ClientId, _setting.ClientSecret, _setting.TokenEndpoint)
        );

        var request = new GraphQLRequest
        {
            Query = @"
            query {
              reporting {
                customerTerminationTrace
              }
            }",
        };

        var response = await authGraphQLClient
            .Request<ReportingResponse>(request)
            .ConfigureAwait(false);

        foreach (var x in response.Data.Reporting.CustomerTerminationTrace)
        {
            _logger.LogInformation(
                "{Text}",
                x);
        }
    }
}
