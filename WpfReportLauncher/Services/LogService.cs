using Serilog;
using System.IO;


namespace WpfReportLauncher.Services
{
    public static class LogService
    {
        public static void Initialize(string baseDir)
        {
            Directory.CreateDirectory(baseDir);
            Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(Path.Combine(baseDir+"/Log", "launcher.log"), rollingInterval: RollingInterval.Day)
            .CreateLogger();
        }
    }
}