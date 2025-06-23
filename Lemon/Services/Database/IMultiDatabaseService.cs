namespace Lemon.Services.Database;

/// <summary>
/// 多数据库服务接口
/// </summary>
public interface IMultiDatabaseService
{
    /// <summary>
    /// 根据名称获取数据库实例
    /// </summary>
    /// <param name="name">数据库名称</param>
    /// <returns>数据库实例</returns>
    IFreeSql GetDatabase(string name);

    /// <summary>
    /// 获取所有数据库名称
    /// </summary>
    /// <returns>数据库名称列表</returns>
    IEnumerable<string> GetDatabaseNames();

    /// <summary>
    /// 检查数据库是否存在
    /// </summary>
    /// <param name="name">数据库名称</param>
    /// <returns>是否存在</returns>
    bool DatabaseExists(string name);

    /// <summary>
    /// 获取活跃的数据库连接数量
    /// </summary>
    /// <returns>活跃连接数量</returns>
    int GetActiveDatabaseCount();
}
