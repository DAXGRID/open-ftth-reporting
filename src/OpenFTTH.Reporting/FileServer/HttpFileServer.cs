using HtmlAgilityPack;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace OpenFTTH.Reporting.FileServer;

internal sealed class HttpFileServer
{
    private readonly HttpClient _httpClient;

    public HttpFileServer(
        HttpClient httpClient,
        string username,
        string password,
        Uri baseAddress)
    {
        httpClient.BaseAddress = baseAddress;
        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Basic",
                BasicAuthToken(username, password));

        _httpClient = httpClient;
    }

    public async Task UploadFile(string localFilePath, string uploadDirPath)
    {
        using var multipartFormContent = new MultipartFormDataContent();
        using var fs = File.OpenRead(localFilePath);
        using var streamContent = new StreamContent(fs);

        multipartFormContent.Add(
            streamContent,
            name: "file",
            fileName: Path.GetFileName(localFilePath));

        var response = await _httpClient
            .PostAsync($"{uploadDirPath}?upload", multipartFormContent)
            .ConfigureAwait(false);

        // The server might return redirect status code
        // We take that as a success after an upload.
        if (!response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.Redirect)
        {
            var errorMessage = await response.Content
                .ReadAsStringAsync()
                .ConfigureAwait(false);

            throw new UploadFileException(
                $"Could not upload file. '{errorMessage}'");
        }
    }

    public async Task CreateDirectory(string name)
    {
        using var formContent = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("name", name),
        });

        var response = await _httpClient
            .PostAsync("?mkdir", formContent)
            .ConfigureAwait(false);

        if (response.StatusCode != HttpStatusCode.Found)
        {
            var errorMessage = await response.Content
                .ReadAsStringAsync()
                .ConfigureAwait(false);

            throw new MakeDirectoryException(
                $"Could not create directory. '{errorMessage}'");
        }
    }

    public async Task DeleteResource(string name, string dirPath)
    {
        var response = await _httpClient
            .PostAsync($"{dirPath}?delete&name={name}&contextquerystring=", null)
            .ConfigureAwait(false);

        if (response.StatusCode != HttpStatusCode.Found)
        {
            var errorMessage = await response.Content
                .ReadAsStringAsync()
                .ConfigureAwait(false);

            throw new DeleteFileException(
                $"Could not delete resource. '{errorMessage}'");
        }
    }

    public async Task<IEnumerable<FileServerFileInfo>> ListFiles(string dirPath)
    {
        var response = await _httpClient
            .GetAsync(dirPath)
            .ConfigureAwait(false);

        var htmlBody = await response.Content
            .ReadAsStringAsync()
            .ConfigureAwait(false);

        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(htmlBody);

        return htmlDocument.DocumentNode
            .Descendants("ul")
            .Where(x => x.Attributes["class"].Value == "item-list has-deletable")
            .First()
            .Descendants()
            .Where(x => x.Attributes["class"]?.Value == "detail")
            .Select(x => x.InnerText
                    .Trim()
                    .Split("\n")
                    .Select(x => x.Trim())
                    .ToArray())
            // We skip first two, since they do not contain output we want.
            .Skip(2)
            .Select(x =>
            {
                var name = x[0];
                var size = SizeShortHandToByteCount(x[1]);
                var created = DateTime.ParseExact(
                    x[2],
                    "yyyy-MM-dd HH:mm",
                    CultureInfo.InvariantCulture);

                return new FileServerFileInfo(name, dirPath, size, created);
            });
    }

    /// <summary>
    /// When size is displayed in the HTML, it might be displayed as 'Ki', 'Mi' or 'Gi'.
    /// This function converts that representation to a byte count representation.
    /// </summary>
    private static long SizeShortHandToByteCount(string text)
    {
        var defaultParseLong = long (string x) =>
        {
            return long.Parse(
                x,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture);
        };

        const string kibiBytes = "K";
        const string mibiBytes = "M";
        const string gibiBytes = "G";

        var textUpperCase = text.ToUpperInvariant();
        if (textUpperCase.Contains(kibiBytes, StringComparison.OrdinalIgnoreCase))
        {
            return defaultParseLong(textUpperCase.Split(kibiBytes)[0]) * 1024;
        }
        else if (textUpperCase.Contains(mibiBytes, StringComparison.OrdinalIgnoreCase))
        {
            return defaultParseLong(textUpperCase.Split(mibiBytes)[0]) * 1024 * 1024;
        }
        else if (textUpperCase.Contains(gibiBytes, StringComparison.OrdinalIgnoreCase))
        {
            return defaultParseLong(textUpperCase.Split(gibiBytes)[0]) * 1024 * 1024 * 1024;
        }
        else // Bytes
        {
            return defaultParseLong(text);
        }
    }

    private static string BasicAuthToken(string username, string password)
    {
        return Convert
            .ToBase64String(ASCIIEncoding.ASCII.GetBytes($"{username}:{password}"));
    }
}
