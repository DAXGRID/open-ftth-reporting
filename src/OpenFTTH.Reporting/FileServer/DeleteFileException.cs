namespace OpenFTTH.Reporting.FileServer;

public class DeleteFileException : Exception
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
