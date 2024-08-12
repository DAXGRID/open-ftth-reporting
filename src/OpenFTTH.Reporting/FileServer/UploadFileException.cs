namespace OpenFTTH.Reporting.FileServer;

public class UploadFileException : Exception
{
    public UploadFileException()
    {
    }

    public UploadFileException(string? message) : base(message)
    {
    }

    public UploadFileException(
        string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
