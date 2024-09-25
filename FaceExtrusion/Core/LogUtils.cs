using System.IO;

namespace FaceExtrusion.Core
{
    internal class LogUtils
    {
        private static ILogger _logger;

        public static void CreateLogger()
        {
            if (_logger != null) { return; }

            // 桌面路径
            string logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "FaceExtrusion");

            Log.Logger = new LoggerConfiguration()
#if DEBUG
                .MinimumLevel.Debug()
#else
                .MinimumLevel.Information()
#endif
                .WriteTo.Async(a => a.File(
                    path: Path.Combine(logPath, "log-.txt"),  // 日志文件路径
                    rollingInterval: RollingInterval.Hour,  // 按小时滚动
                    restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information,  // 最低记录级别
                    retainedFileCountLimit: 10,  // 保留文件数量
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}"  // 输出模板
                    ))
                .CreateLogger();

            _logger = Log.Logger;

            // 全局异常处理
            AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            {
                Exception e = (Exception)args.ExceptionObject;
                Log.Fatal(e, "Domain unhandled exception");
            };
        }

        public static void CloseLogger()
        {
            if (_logger == null) { return; }
            _logger = null;

            Log.CloseAndFlush();
            AppDomain.CurrentDomain.UnhandledException -= (_, args) =>
            {
                Exception e = (Exception)args.ExceptionObject;
                Log.Fatal(e, "Domain unhandled exception");
            };
        }
    }
}