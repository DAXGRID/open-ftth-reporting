namespace OpenFTTH.Reporting;

internal static class Program
{
    public static async Task Main()
    {
        var settings = AppSetting.Load<Setting>();
        var logger = LoggerFactory.Create("Reporting");
        var startUp = new StartUp(logger, settings);
        await startUp.StartAsync().ConfigureAwait(false);
    }
}
