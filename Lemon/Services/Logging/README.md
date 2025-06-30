# Lemon 日志服务

Lemon 提供了基于 Serilog 的日志记录功能，支持控制台输出、文件输出和异步日志记录。

## 功能特性

- ✅ 控制台日志输出
- ✅ 文件日志输出
- ✅ 异步日志记录
- ✅ 灵活的日志级别配置
- ✅ 自定义输出模板
- ✅ 日志文件滚动和大小限制

## 快速开始

### 1. 配置服务

在 `Program.cs` 中添加 Serilog 配置：

```csharp
var builder = WebApplication.CreateBuilder(args);

// 使用 Lemon Serilog 配置
builder.UseLemonSerilog();

// 或者使用自定义配置
builder.UseLemonSerilog(options =>
{
    options.EnableAsyncLogging = true;
    options.AsyncBufferSize = 5000;
    options.EnableFileLogging = true;
    options.LogFilePath = "logs/myapp-.log";
});

var app = builder.Build();
```

### 2. 配置文件设置

在 `appsettings.json` 中添加 Serilog 配置：

```json
{
  "Serilog": {
    "EnableAsyncLogging": true,
    "AsyncBufferSize": 10000,
    "AsyncBlockWhenFull": false,
    "EnableFileLogging": true,
    "EnableConsoleLogging": true,
    "LogFilePath": "logs/app-.log",
    "RollingInterval": "Day",
    "RetainedFileCountLimit": 30,
    "FileSizeLimitBytes": 10485760,
    "RollOnFileSizeLimit": true,
    "Shared": true,
    "FlushToDiskIntervalSeconds": 1,
    "MinimumLevel": "Information",
    "MinimumLevelOverrides": {
      "Microsoft": "Warning",
      "Microsoft.AspNetCore": "Warning",
      "System": "Warning"
    },
    "OutputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
  }
}
```

## 异步日志配置

### 配置选项

| 配置项               | 类型 | 默认值 | 说明                 |
| -------------------- | ---- | ------ | -------------------- |
| `EnableAsyncLogging` | bool | false  | 是否启用异步日志记录 |
| `AsyncBufferSize`    | int  | 10000  | 异步日志缓冲区大小   |
| `AsyncBlockWhenFull` | bool | false  | 缓冲区满时是否阻塞   |

### 异步日志优势

1. **提高性能**: 日志写入操作不会阻塞应用程序主线程
2. **更好的响应性**: 减少日志记录对请求处理的影响
3. **批量写入**: 支持批量写入以提高 I/O 效率

### 使用建议

- **开发环境**: 可以禁用异步日志以便于调试
- **生产环境**: 建议启用异步日志以提高性能
- **高并发场景**: 适当增加 `AsyncBufferSize` 的值
- **内存敏感场景**: 设置 `AsyncBlockWhenFull` 为 `true` 以防止内存溢出

## 配置参数说明

### 基础配置

- `EnableFileLogging`: 是否启用文件日志输出
- `EnableConsoleLogging`: 是否启用控制台日志输出
- `LogFilePath`: 日志文件路径模板
- `RollingInterval`: 日志滚动间隔 (Day, Hour, Minute, Month, Year, Infinite)
- `RetainedFileCountLimit`: 保留的日志文件数量
- `FileSizeLimitBytes`: 单个日志文件大小限制
- `RollOnFileSizeLimit`: 是否在文件大小达到限制时滚动
- `Shared`: 是否启用共享文件访问
- `FlushToDiskIntervalSeconds`: 刷新到磁盘的间隔（秒）

### 日志级别配置

- `MinimumLevel`: 最小日志级别
- `MinimumLevelOverrides`: 针对特定命名空间的日志级别重写

### 输出格式

- `OutputTemplate`: 自定义日志输出模板

## 示例代码

```csharp
public class WeatherForecastController : ControllerBase
{
    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Get()
    {
        _logger.LogInformation("获取天气预报数据");

        try
        {
            var result = GetWeatherData();
            _logger.LogInformation("成功获取天气数据，共 {Count} 条记录", result.Count);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取天气数据时发生错误");
            return StatusCode(500, "服务器内部错误");
        }
    }
}
```

## 性能优化建议

1. **异步日志**: 在生产环境中启用异步日志记录
2. **日志级别**: 适当设置日志级别，避免记录过多无用信息
3. **文件滚动**: 合理配置文件滚动策略，避免单个文件过大
4. **缓冲区大小**: 根据应用负载调整异步缓冲区大小
5. **刷新间隔**: 平衡数据安全性和性能，适当调整刷新间隔
