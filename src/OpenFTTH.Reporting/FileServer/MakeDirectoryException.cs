namespace OpenFTTH.Reporting.FileServer;

public class MakeDirectoryException : Exception
{
    public MakeDirectoryException()
    {
    }

    public MakeDirectoryException(string? message) : base(message)
    {
    }

    public MakeDirectoryException(
        string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
