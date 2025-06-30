using Lemon.Services.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;

namespace Lemon.Services.Extensions;

/// <summary>
/// WebApplicationBuilder 扩展方法
/// </summary>
public static class WebApplicationBuilderExtensions
{
    /// <summary>
    /// 使用 Serilog 日志配置
    /// </summary>
    /// <param name="builder">WebApplicationBuilder</param>
    /// <param name="configureOptions">配置选项委托</param>
    /// <returns>WebApplicationBuilder</returns>
    public static WebApplicationBuilder UseLemonSerilog(
        this WebApplicationBuilder builder,
        Action<SerilogOptions>? configureOptions = null
    )
    {
        // 从配置文件读取 Serilog 选项
        var serilogOptions = new SerilogOptions();
        builder.Configuration.GetSection("Serilog").Bind(serilogOptions);

        // 应用自定义配置
        configureOptions?.Invoke(serilogOptions);

        // 配置 Serilog
        builder.Host.UseSerilog(
            (context, services, configuration) =>
            {
                configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext();

                // 设置最小日志级别
                if (
                    Enum.TryParse<LogEventLevel>(
                        serilogOptions.MinimumLevel,
                        true,
                        out var minLevel
                    )
                )
                {
                    configuration.MinimumLevel.Is(minLevel);
                }

                // 设置日志级别重写
                foreach (var @override in serilogOptions.MinimumLevelOverrides)
                {
                    if (Enum.TryParse<LogEventLevel>(@override.Value, true, out var level))
                    {
                        configuration.MinimumLevel.Override(@override.Key, level);
                    }
                }

                // 添加控制台输出
                if (serilogOptions.EnableConsoleLogging)
                {
                    if (serilogOptions.EnableAsyncLogging)
                    {
                        configuration.WriteTo.Async(
                            a => a.Console(outputTemplate: serilogOptions.OutputTemplate),
                            bufferSize: serilogOptions.AsyncBufferSize,
                            blockWhenFull: serilogOptions.AsyncBlockWhenFull
                        );
                    }
                    else
                    {
                        configuration.WriteTo.Console(
                            outputTemplate: serilogOptions.OutputTemplate
                        );
                    }
                }

                // 添加文件输出
                if (serilogOptions.EnableFileLogging)
                {
                    var rollingInterval = ParseRollingInterval(serilogOptions.RollingInterval);

                    if (serilogOptions.EnableAsyncLogging)
                    {
                        configuration.WriteTo.Async(
                            a =>
                                a.File(
                                    path: serilogOptions.LogFilePath,
                                    rollingInterval: rollingInterval,
                                    retainedFileCountLimit: serilogOptions.RetainedFileCountLimit,
                                    fileSizeLimitBytes: serilogOptions.FileSizeLimitBytes,
                                    rollOnFileSizeLimit: serilogOptions.RollOnFileSizeLimit,
                                    shared: serilogOptions.Shared,
                                    flushToDiskInterval: TimeSpan.FromSeconds(
                                        serilogOptions.FlushToDiskIntervalSeconds
                                    ),
                                    outputTemplate: serilogOptions.OutputTemplate
                                ),
                            bufferSize: serilogOptions.AsyncBufferSize,
                            blockWhenFull: serilogOptions.AsyncBlockWhenFull
                        );
                    }
                    else
                    {
                        configuration.WriteTo.File(
                            path: serilogOptions.LogFilePath,
                            rollingInterval: rollingInterval,
                            retainedFileCountLimit: serilogOptions.RetainedFileCountLimit,
                            fileSizeLimitBytes: serilogOptions.FileSizeLimitBytes,
                            rollOnFileSizeLimit: serilogOptions.RollOnFileSizeLimit,
                            shared: serilogOptions.Shared,
                            flushToDiskInterval: TimeSpan.FromSeconds(
                                serilogOptions.FlushToDiskIntervalSeconds
                            ),
                            outputTemplate: serilogOptions.OutputTemplate
                        );
                    }
                }
            }
        );

        return builder;
    }

    /// <summary>
    /// 解析滚动间隔字符串
    /// </summary>
    private static RollingInterval ParseRollingInterval(string interval)
    {
        return interval.ToLower() switch
        {
            "minute" => RollingInterval.Minute,
            "hour" => RollingInterval.Hour,
            "day" => RollingInterval.Day,
            "month" => RollingInterval.Month,
            "year" => RollingInterval.Year,
            "infinite" => RollingInterval.Infinite,
            _ => RollingInterval.Day,
        };
    }
}
