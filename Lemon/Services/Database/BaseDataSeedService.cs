using FreeSql;
using Microsoft.Extensions.Logging;

namespace Lemon.Services.Database;

/// <summary>
/// 基于FreeSql的数据库填充服务基类
/// </summary>
public abstract class BaseDataSeedService(IFreeSql freeSql, ILogger logger) : IDataSeedService
{
    protected readonly IFreeSql _freeSql = freeSql;
    protected readonly ILogger _logger = logger;

    /// <summary>
    /// 执行数据填充
    /// </summary>
    public async Task SeedAsync()
    {
        try
        {
            _logger.LogInformation("开始执行数据填充: {Name}", Name);

            // 检查数据是否已存在
            if (await IsDataExistsAsync())
            {
                _logger.LogInformation("数据已存在，跳过填充: {Name}", Name);
                return;
            }

            // 执行具体的数据填充逻辑
            await SeedDataAsync();

            _logger.LogInformation("数据填充完成: {Name}", Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "数据填充失败: {Name}", Name);
            throw;
        }
    }

    /// <summary>
    /// 执行具体的数据填充逻辑
    /// </summary>
    protected abstract Task SeedDataAsync();

    /// <summary>
    /// 填充名称
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// 获取填充优先级（数字越小优先级越高）
    /// </summary>
    public virtual int Priority => 1000;

    /// <summary>
    /// 检查数据是否已存在，避免重复填充
    /// </summary>
    /// <returns>如果数据已存在返回true，否则返回false</returns>
    protected virtual async Task<bool> IsDataExistsAsync()
    {
        // 默认实现始终返回false，子类可以重写此方法
        await Task.CompletedTask;
        return false;
    }

    /// <summary>
    /// 检查表中是否有数据
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    /// <returns>如果表中有数据返回true，否则返回false</returns>
    protected async Task<bool> HasDataAsync<T>()
        where T : class
    {
        var count = await _freeSql.Select<T>().CountAsync();
        return count > 0;
    }

    /// <summary>
    /// 批量插入数据
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    /// <param name="entities">要插入的实体列表</param>
    /// <returns>插入的记录数</returns>
    protected async Task<int> BulkInsertAsync<T>(List<T> entities)
        where T : class
    {
        if (entities == null || entities.Count == 0)
            return 0;

        var result = await _freeSql.Insert(entities).ExecuteAffrowsAsync();
        _logger.LogInformation("批量插入 {Count} 条 {EntityType} 记录", result, typeof(T).Name);
        return result;
    }

    /// <summary>
    /// 开始事务执行填充
    /// </summary>
    /// <param name="seedAction">填充操作</param>
    protected async Task ExecuteInTransactionAsync(Func<Task> seedAction)
    {
        using var transaction = _freeSql.Ado.TransactionCurrentThread;
        try
        {
            await seedAction();
        }
        catch
        {
            throw;
        }
    }
}
