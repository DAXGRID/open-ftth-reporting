using System.Globalization;
using System.IO.Compression;
using GraphQL;
using Microsoft.Extensions.Logging;
using OpenFTTH.Reporting.FileServer;

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

        var csvFilePath = $"{Path.GetTempPath()}/openftth_ny_trace.csv";
        using StreamWriter csvFile = new(csvFilePath);

        foreach (var customerTerminationTrace in response.Data.Reporting.CustomerTerminationTrace)
        {
            await csvFile.WriteLineAsync(customerTerminationTrace).ConfigureAwait(false);
        }

        csvFile.Close();

        var zipFilePath = $"{Path.GetTempPath()}/report_{DateTime.Now.ToString("dd_MM_yyyy", CultureInfo.InvariantCulture)}";

        _logger.LogInformation(
            "Zipping {CsvFilePath} into name {ZipName}.",
            csvFilePath,
            zipFilePath);

        ZipFile.CreateFromDirectory(
            csvFilePath,
            zipFilePath,
            CompressionLevel.SmallestSize,
            false);

        using var httpClientHandler = new HttpClientHandler
        {
            // The file-server might return redirects,
            // we do not want to follow the redirects.
            AllowAutoRedirect = false,
            CheckCertificateRevocationList = true,
        };

        using var fileServerHttpClient = new HttpClient(httpClientHandler);

        var httpFileServer = new HttpFileServer(
            fileServerHttpClient,
            _setting.FileServer.Username,
            _setting.FileServer.Password,
            new Uri(_setting.FileServer.HostAddress));

        _logger.LogInformation(
            "Creating directory on fileserver {DirectoryName}.", _setting.UploadPath);

        await httpFileServer
            .CreateDirectory(
                _setting.UploadPath)
            .ConfigureAwait(false);

        _logger.LogInformation("Uploading file {FileName}.", zipFilePath);

        await httpFileServer
            .UploadFile(
                zipFilePath,
                _setting.UploadPath)
            .ConfigureAwait(false);

        var files = await httpFileServer
            .ListFiles(_setting.UploadPath)
            .ConfigureAwait(false);

        // We only want to keep the 3 newest reports.
        foreach (var file in files.OrderBy(x => x.Created).Take(files.Count() - 3))
        {
            _logger.LogInformation("Cleanup old reporting files {FileName}.", file.Name);
            await httpFileServer
                .DeleteResource(file.Name, _setting.UploadPath)
                .ConfigureAwait(false);
        }

        _logger.LogInformation("Finished reporting.");
    }
}
