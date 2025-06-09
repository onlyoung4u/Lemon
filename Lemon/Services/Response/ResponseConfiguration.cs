namespace Lemon.Services.Response;

/// <summary>
/// 响应配置模型
/// </summary>
public class ResponseConfiguration
{
    /// <summary>
    /// 配置节名称
    /// </summary>
    public const string SectionName = "Response";

    /// <summary>
    /// 自定义状态码和消息映射
    /// </summary>
    public Dictionary<string, string> CustomMessages { get; set; } = [];
}

/// <summary>
/// 响应消息配置项
/// </summary>
public class ResponseMessageItem
{
    /// <summary>
    /// 状态码
    /// </summary>
    public int Code { get; set; }

    /// <summary>
    /// 消息
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
