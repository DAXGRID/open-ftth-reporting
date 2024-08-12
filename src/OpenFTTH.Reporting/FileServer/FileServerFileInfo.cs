namespace OpenFTTH.Reporting.FileServer;

internal sealed record FileServerFileInfo
{
    public string Name { get; init; }
    public string DirPath { get; init; }
    public long Size { get; init; }
    public DateTime Created { get; init; }

    public FileServerFileInfo(string name, string dirPath, long size, DateTime created)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                $"'{nameof(name)}' cannot be null or whitespace.",
                nameof(name));
        }

        if (string.IsNullOrWhiteSpace(dirPath))
        {
            throw new ArgumentException(
                $"'{nameof(dirPath)}' cannot be null or whitespace.",
                nameof(dirPath));
        }

        if (created == default)
        {
            throw new ArgumentException(
                $"'{nameof(created)}' cannot be default date.",
                nameof(created));
        }

        Name = name;
        DirPath = dirPath;
        Size = size;
        Created = created;
    }
}
