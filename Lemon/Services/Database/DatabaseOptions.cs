namespace Lemon.Services.Database;

/// <summary>
/// 单个数据库配置
/// </summary>
public class DatabaseConfig
{
    /// <summary>
    /// 数据库名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 连接字符串
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// 数据库类型字符串
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// FreeSql数据库类型枚举
    /// </summary>
    public FreeSql.DataType DbType { get; set; }

    /// <summary>
    /// 是否使用连接池（推荐开启）
    /// </summary>
    public bool ConnectionPool { get; set; } = true;

    /// <summary>
    /// 是否自动同步结构（仅在开发环境建议开启）
    /// </summary>
    public bool AutoSyncStructure { get; set; } = false;

    /// <summary>
    /// 是否启用监控命令（仅在开发环境）
    /// </summary>
    public bool EnableMonitor { get; set; } = false;
}
