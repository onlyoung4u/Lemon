using System.Collections.Concurrent;
using FreeSql;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lemon.Services.Database;

/// <summary>
/// 多数据库服务实现
/// </summary>
public class MultiDatabaseService : IMultiDatabaseService, IDisposable
{
    private readonly List<DatabaseConfig> _options;
    private readonly ConcurrentDictionary<string, IFreeSql> _databases;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly ILogger<MultiDatabaseService> _logger;
    private bool _disposed = false;

    public MultiDatabaseService(
        IOptions<List<DatabaseConfig>> options,
        IHostEnvironment hostEnvironment,
        ILogger<MultiDatabaseService> logger
    )
    {
        _options = options.Value;
        _databases = new ConcurrentDictionary<string, IFreeSql>();
        _hostEnvironment = hostEnvironment;
        _logger = logger;

        _logger.LogInformation("多数据库服务已初始化，共配置 {Count} 个数据库", _options.Count);
    }

    /// <summary>
    /// 根据名称获取数据库实例
    /// </summary>
    public IFreeSql GetDatabase(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException("数据库名称不能为空", nameof(name));
        }

        if (!DatabaseExists(name))
        {
            throw new InvalidOperationException($"数据库配置 '{name}' 未找到");
        }

        try
        {
            var database = _databases.GetOrAdd(name, CreateDatabase);
            _logger.LogDebug("获取数据库实例: {DatabaseName}", name);
            return database;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取数据库实例失败: {DatabaseName}", name);
            throw;
        }
    }

    /// <summary>
    /// 获取所有数据库名称
    /// </summary>
    public IEnumerable<string> GetDatabaseNames()
    {
        return _options.Select(k => k.Name).Distinct();
    }

    /// <summary>
    /// 检查数据库是否存在
    /// </summary>
    public bool DatabaseExists(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return false;
        }

        return _options.Any(k => k.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// 获取活跃的数据库连接数量
    /// </summary>
    public int GetActiveDatabaseCount()
    {
        return _databases.Count;
    }

    /// <summary>
    /// 创建数据库实例
    /// </summary>
    private IFreeSql CreateDatabase(string name)
    {
        _logger.LogInformation("正在创建数据库实例: {DatabaseName}", name);

        var config =
            _options.FirstOrDefault(k => k.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"数据库配置 '{name}' 未找到");

        try
        {
            var isDevelopment = _hostEnvironment.IsDevelopment();

            var builder = new FreeSqlBuilder()
                .UseConnectionString(config.DbType, config.ConnectionString)
                .UseAdoConnectionPool(config.ConnectionPool)
                .UseAutoSyncStructure(isDevelopment && config.AutoSyncStructure);

            if (isDevelopment && config.EnableMonitor)
            {
                builder.UseMonitorCommand(cmd =>
                    _logger.LogDebug(
                        "[{DatabaseName}] SQL执行: {CommandText}",
                        name,
                        cmd.CommandText
                    )
                );
            }

            var freeSql = builder.Build();

            _logger.LogInformation(
                "数据库实例创建成功: {DatabaseName}, 类型: {Type}",
                name,
                config.Type
            );

            return freeSql;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建数据库实例失败: {DatabaseName}", name);

            throw new InvalidOperationException($"创建数据库实例 '{name}' 失败", ex);
        }
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            _logger.LogInformation("正在释放多数据库服务资源...");

            foreach (var kvp in _databases)
            {
                try
                {
                    kvp.Value?.Dispose();
                    _logger.LogDebug("数据库实例已释放: {DatabaseName}", kvp.Key);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "释放数据库实例失败: {DatabaseName}", kvp.Key);
                }
            }

            _databases.Clear();
            _disposed = true;

            _logger.LogInformation("多数据库服务资源释放完成");
        }

        GC.SuppressFinalize(this);
    }
}
