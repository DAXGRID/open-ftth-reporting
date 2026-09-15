namespace OpenFTTH.Reporting.FileServer;

internal sealed class DeleteFileException : Exception
{
    public DeleteFileException()
    {
    }

    public DeleteFileException(string? message) : base(message)
    {
    }

    public DeleteFileException(
        string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
