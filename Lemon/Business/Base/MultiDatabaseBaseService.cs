using Lemon.Services.Database;

namespace Lemon.Business.Base;

/// <summary>
/// 支持多数据库的基础服务类
/// </summary>
public abstract class MultiDatabaseBaseService(
    IFreeSql freeSql,
    IMultiDatabaseService multiDatabase
) : BaseService(freeSql)
{
    protected readonly IMultiDatabaseService _multiDatabase = multiDatabase;

    /// <summary>
    /// 获取指定名称的数据库实例
    /// </summary>
    /// <param name="databaseName">数据库名称</param>
    /// <returns>数据库实例</returns>
    protected IFreeSql GetDatabase(string databaseName)
    {
        return _multiDatabase.GetDatabase(databaseName);
    }
}
