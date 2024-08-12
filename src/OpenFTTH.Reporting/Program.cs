using Microsoft.Extensions.Logging;

namespace OpenFTTH.Reporting;

internal static class Program
{
    public static void Main()
    {
        var settings = AppSetting.Load<Setting>();
        var logger = LoggerFactory.Create("Reporting");
        logger.LogInformation("Hello, World!");
    }
}
