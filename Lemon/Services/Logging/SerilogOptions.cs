namespace Lemon.Services.Logging;

/// <summary>
/// Serilog 配置选项
/// </summary>
public class SerilogOptions
{
    /// <summary>
    /// 是否启用异步日志
    /// </summary>
    public bool EnableAsyncLogging { get; set; } = false;

    /// <summary>
    /// 异步日志缓冲区大小
    /// </summary>
    public int AsyncBufferSize { get; set; } = 10000;

    /// <summary>
    /// 是否阻塞当缓冲区已满
    /// </summary>
    public bool AsyncBlockWhenFull { get; set; } = false;

    /// <summary>
    /// 是否启用文件日志
    /// </summary>
    public bool EnableFileLogging { get; set; } = true;

    /// <summary>
    /// 是否启用控制台日志
    /// </summary>
    public bool EnableConsoleLogging { get; set; } = true;

    /// <summary>
    /// 日志文件路径模板
    /// 默认: logs/app-.log (会自动添加日期)
    /// </summary>
    public string LogFilePath { get; set; } = "logs/app-.log";

    /// <summary>
    /// 日志滚动间隔
    /// Day, Hour, Minute, Month, Year, Infinite
    /// </summary>
    public string RollingInterval { get; set; } = "Day";

    /// <summary>
    /// 保留的日志文件数量
    /// </summary>
    public int? RetainedFileCountLimit { get; set; } = 30;

    /// <summary>
    /// 单个日志文件大小限制（字节）
    /// 默认: 10MB
    /// </summary>
    public long? FileSizeLimitBytes { get; set; } = 10 * 1024 * 1024;

    /// <summary>
    /// 是否在文件大小达到限制时滚动
    /// </summary>
    public bool RollOnFileSizeLimit { get; set; } = true;

    /// <summary>
    /// 是否启用共享文件访问
    /// </summary>
    public bool Shared { get; set; } = true;

    /// <summary>
    /// 刷新到磁盘的间隔（秒）
    /// </summary>
    public int FlushToDiskIntervalSeconds { get; set; } = 1;

    /// <summary>
    /// 最小日志级别
    /// </summary>
    public string MinimumLevel { get; set; } = "Information";

    /// <summary>
    /// 日志级别重写配置
    /// </summary>
    public Dictionary<string, string> MinimumLevelOverrides { get; set; } =
        new()
        {
            ["Microsoft"] = "Warning",
            ["Microsoft.AspNetCore"] = "Warning",
            ["System"] = "Warning",
        };

    /// <summary>
    /// 日志输出模板
    /// </summary>
    public string OutputTemplate { get; set; } =
        "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}";
}
