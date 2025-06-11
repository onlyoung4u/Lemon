using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lemon.Services.Database;

/// <summary>
/// 数据库填充管理器
/// </summary>
public class DataSeedManager(IServiceProvider serviceProvider, ILogger<DataSeedManager> logger)
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly ILogger<DataSeedManager> _logger = logger;

    /// <summary>
    /// 执行所有数据填充
    /// </summary>
    public async Task SeedAllAsync()
    {
        try
        {
            _logger.LogInformation("开始执行数据库填充...");

            // 获取所有数据填充服务
            var seedServices = _serviceProvider.GetServices<IDataSeedService>();

            if (!seedServices.Any())
            {
                _logger.LogInformation("未找到任何数据填充服务");
                return;
            }

            // 按优先级排序
            var orderedServices = seedServices.OrderBy(s => s.Priority).ToList();

            _logger.LogInformation("找到 {Count} 个数据填充服务", orderedServices.Count);

            // 执行填充
            foreach (var service in orderedServices)
            {
                try
                {
                    _logger.LogInformation(
                        "执行数据填充: {Name} (优先级: {Priority})",
                        service.Name,
                        service.Priority
                    );
                    await service.SeedAsync();
                    _logger.LogInformation("数据填充完成: {Name}", service.Name);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "数据填充失败: {Name}", service.Name);
                    throw;
                }
            }

            _logger.LogInformation("所有数据库填充执行完成");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "数据库填充过程中发生错误");
            throw;
        }
    }

    /// <summary>
    /// 执行指定类型的数据填充
    /// </summary>
    /// <typeparam name="T">数据填充服务类型</typeparam>
    public async Task SeedAsync<T>()
        where T : class, IDataSeedService
    {
        var service = _serviceProvider.GetService<T>();
        if (service == null)
        {
            _logger.LogWarning("未找到数据填充服务: {ServiceType}", typeof(T).Name);
            return;
        }

        await service.SeedAsync();
    }
}
