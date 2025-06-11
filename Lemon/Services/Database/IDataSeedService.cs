namespace Lemon.Services.Database;

/// <summary>
/// 数据库填充服务接口
/// </summary>
public interface IDataSeedService
{
    /// <summary>
    /// 执行数据填充
    /// </summary>
    Task SeedAsync();

    /// <summary>
    /// 获取填充优先级（数字越小优先级越高）
    /// </summary>
    int Priority { get; }

    /// <summary>
    /// 填充名称
    /// </summary>
    string Name { get; }
}
