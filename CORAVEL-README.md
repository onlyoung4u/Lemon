# Coravel 定时任务使用指南

本文档详细介绍如何在 Lemon 项目中使用 Coravel 库实现类似 crontab 的定时任务功能。

## 📋 目录

- [概述](#概述)
- [安装配置](#安装配置)
- [任务示例](#任务示例)
- [调度配置](#调度配置)
- [常用调度模式](#常用调度模式)
- [最佳实践](#最佳实践)
- [故障排除](#故障排除)

## 🎯 概述

Coravel 是一个 .NET 轻量级任务调度库，提供了类似 Linux crontab 的功能，但更加简单易用。本项目中已经配置了以下示例任务：

- **数据清理任务** - 每天凌晨 2 点执行
- **每日报告任务** - 每天上午 9 点执行
- **系统健康检查** - 每 5 分钟执行一次
- **每周备份任务** - 每周日凌晨 1 点执行

## 🔧 安装配置

### 1. NuGet 包安装

项目已经安装了 Coravel 包，如果需要在新项目中使用，请安装：

```bash
dotnet add package Coravel
```

### 2. 服务注册

在 `Program.cs` 中注册 Coravel 服务：

```csharp
// 添加调度器服务
builder.Services.AddScheduler();

// 注册定时任务
builder.Services.AddTransient<DataCleanupTask>();
builder.Services.AddTransient<DailyReportTask>();
builder.Services.AddTransient<HealthCheckTask>();
builder.Services.AddTransient<WeeklyBackupTask>();
```

### 3. 任务调度配置

在应用启动时配置任务调度：

```csharp
app.Services.UseScheduler(scheduler =>
{
    // 数据清理任务 - 每天凌晨 2 点执行
    scheduler.Schedule<DataCleanupTask>().DailyAtHour(2);

    // 每日报告任务 - 每天上午 9 点执行
    scheduler.Schedule<DailyReportTask>().DailyAtHour(9);

    // 系统健康检查任务 - 每 5 分钟执行一次
    scheduler.Schedule<HealthCheckTask>().EveryFiveMinutes();

    // 每周备份任务 - 每小时检查一次，在任务内部判断是否执行
    scheduler.Schedule<WeeklyBackupTask>().Hourly();
});
```

## 📝 任务示例

### 创建自定义任务

所有任务都需要实现 `IInvocable` 接口：

```csharp
using Coravel.Invocable;

public class CustomTask : IInvocable
{
    private readonly ILogger<CustomTask> _logger;

    public CustomTask(ILogger<CustomTask> logger)
    {
        _logger = logger;
    }

    public async Task Invoke()
    {
        _logger.LogInformation("开始执行自定义任务...");

        try
        {
            // 在这里实现任务逻辑
            await YourTaskLogicAsync();

            _logger.LogInformation("自定义任务执行完成");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "执行自定义任务时发生错误");
            throw; // 重新抛出异常以便 Coravel 记录失败
        }
    }

    private async Task YourTaskLogicAsync()
    {
        // 实现具体的任务逻辑
        await Task.Delay(1000);
    }
}
```

## ⏰ 调度配置

### 基本调度方法

```csharp
// 每分钟执行
scheduler.Schedule<YourTask>().EveryMinute();

// 每5分钟执行
scheduler.Schedule<YourTask>().EveryFiveMinutes();

// 每小时执行
scheduler.Schedule<YourTask>().Hourly();

// 每天执行
scheduler.Schedule<YourTask>().Daily();

// 每天特定时间执行
scheduler.Schedule<YourTask>().DailyAtHour(9); // 每天上午9点

// 每周执行
scheduler.Schedule<YourTask>().Weekly();
```

### 高级调度配置

对于更复杂的调度需求，可以在任务内部添加时间检查逻辑：

```csharp
public async Task Invoke()
{
    var now = DateTime.Now;

    // 只在工作日执行
    if (now.DayOfWeek == DayOfWeek.Saturday || now.DayOfWeek == DayOfWeek.Sunday)
    {
        _logger.LogDebug("跳过周末执行");
        return;
    }

    // 只在特定时间范围内执行
    if (now.Hour < 9 || now.Hour > 17)
    {
        _logger.LogDebug("跳过非工作时间执行");
        return;
    }

    // 执行任务逻辑
    await ExecuteTaskAsync();
}
```

## 🚀 常用调度模式

### 1. 数据清理任务

```csharp
/// <summary>
/// 每天凌晨执行数据清理
/// </summary>
public class DataCleanupTask : IInvocable
{
    public async Task Invoke()
    {
        // 清理旧日志
        // 清理临时文件
        // 清理过期数据
    }
}

// 调度配置
scheduler.Schedule<DataCleanupTask>().DailyAtHour(2);
```

### 2. 报告生成任务

```csharp
/// <summary>
/// 每日报告生成
/// </summary>
public class DailyReportTask : IInvocable
{
    public async Task Invoke()
    {
        // 生成日报
        // 发送邮件通知
        // 保存到文件系统
    }
}

// 调度配置
scheduler.Schedule<DailyReportTask>().DailyAtHour(9);
```

### 3. 监控检查任务

```csharp
/// <summary>
/// 系统健康检查
/// </summary>
public class HealthCheckTask : IInvocable
{
    public async Task Invoke()
    {
        // 检查数据库连接
        // 检查外部API状态
        // 检查磁盘空间
        // 发送告警通知
    }
}

// 调度配置
scheduler.Schedule<HealthCheckTask>().EveryFiveMinutes();
```

### 4. 备份任务

```csharp
/// <summary>
/// 定期备份任务
/// </summary>
public class BackupTask : IInvocable
{
    public async Task Invoke()
    {
        var now = DateTime.Now;

        // 每周日凌晨1点执行备份
        if (now.DayOfWeek == DayOfWeek.Sunday && now.Hour == 1)
        {
            await PerformBackupAsync();
        }
    }
}

// 调度配置
scheduler.Schedule<BackupTask>().Hourly();
```

## 💡 最佳实践

### 1. 错误处理

```csharp
public async Task Invoke()
{
    try
    {
        await ExecuteTaskLogicAsync();
        _logger.LogInformation("任务执行成功");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "任务执行失败");

        // 可选：发送告警通知
        await SendAlertAsync(ex);

        // 重新抛出异常让 Coravel 记录失败
        throw;
    }
}
```

### 2. 任务幂等性

确保任务可以安全地重复执行：

```csharp
public async Task Invoke()
{
    // 检查是否已经执行过
    var today = DateTime.Today;
    var hasExecutedToday = await CheckExecutionRecordAsync(today);

    if (hasExecutedToday)
    {
        _logger.LogInformation("今日已执行，跳过");
        return;
    }

    await ExecuteTaskAsync();
    await RecordExecutionAsync(today);
}
```

### 3. 资源管理

```csharp
public async Task Invoke()
{
    using var scope = _serviceProvider.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<DbContext>();

    // 使用作用域内的服务执行任务
    await ProcessDataAsync(dbContext);
}
```

### 4. 配置参数

```csharp
public class ConfigurableTask : IInvocable
{
    private readonly IConfiguration _configuration;

    public async Task Invoke()
    {
        var batchSize = _configuration.GetValue<int>("Tasks:BatchSize", 100);
        var timeout = _configuration.GetValue<int>("Tasks:TimeoutSeconds", 300);

        // 使用配置参数执行任务
    }
}
```

## 🔍 故障排除

### 1. 任务未执行

检查以下几点：

- 确认任务已正确注册到 DI 容器
- 确认调度配置语法正确
- 检查应用程序日志中的错误信息
- 确认应用程序持续运行

### 2. 任务执行失败

```csharp
// 添加详细的日志记录
public async Task Invoke()
{
    _logger.LogInformation("任务开始执行，时间：{Time}", DateTime.Now);

    try
    {
        // 任务逻辑
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "任务执行异常：{Message}", ex.Message);
        throw;
    }
    finally
    {
        _logger.LogInformation("任务执行结束，时间：{Time}", DateTime.Now);
    }
}
```

### 3. 性能优化

```csharp
public async Task Invoke()
{
    var stopwatch = Stopwatch.StartNew();

    try
    {
        await ExecuteTaskAsync();
    }
    finally
    {
        stopwatch.Stop();
        _logger.LogInformation("任务执行耗时：{ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
    }
}
```

## 📚 更多资源

- [Coravel 官方文档](https://docs.coravel.net/)
- [Coravel GitHub 仓库](https://github.com/jamesmh/coravel)
- [.NET 后台任务最佳实践](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services)

## 🎯 下一步

1. 根据业务需求创建自定义任务
2. 配置合适的调度时间
3. 添加监控和告警
4. 设置任务执行日志记录
5. 考虑任务的容错和重试机制

---

> 💡 **提示**: 在生产环境中，建议使用专门的任务调度平台（如 Hangfire、Quartz.NET）处理复杂的定时任务需求。Coravel 更适合轻量级的应用场景。
