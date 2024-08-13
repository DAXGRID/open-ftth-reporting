using System.Globalization;
using System.IO.Compression;
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
        _logger.LogInformation("Getting reports.");

        using var httpClient = new HttpClient()
        {
            BaseAddress = new Uri(_setting.ReportApiBaseAddress)
        };

        var response = await httpClient
            .GetAsync(
                "/api/Report/CustomerTerminationReport",
                HttpCompletionOption.ResponseHeadersRead)
            .ConfigureAwait(false);

        response.EnsureSuccessStatusCode();

        var fileDateName = $"{DateTime.Now.ToString("dd_MM_yyyy", CultureInfo.InvariantCulture)}";

        var csvFilePath = $"{Path.GetTempPath()}trace_installations_{fileDateName}.csv";

        _logger.LogInformation("Got the report, starts writing file to {FilePath}.", csvFilePath);

        using (var fileStream = new FileStream(csvFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            using (Stream responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
            {
                await responseStream.CopyToAsync(fileStream).ConfigureAwait(false);
            }
        }

        var zipFilePath = $"{Path.GetTempPath()}report_{fileDateName}.zip";

        _logger.LogInformation(
            "Zipping {CsvFilePath} into name {ZipName}.",
            csvFilePath,
            zipFilePath);

        using (var fs = new FileStream(zipFilePath,FileMode.Create))
        using (var arch = new ZipArchive(fs, ZipArchiveMode.Create))
        {
            arch.CreateEntryFromFile(csvFilePath, Path.GetFileName(csvFilePath));
        }

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
