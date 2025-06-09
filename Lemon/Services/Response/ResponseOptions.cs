namespace Lemon.Services.Response;

/// <summary>
/// 响应配置选项
/// </summary>
public class ResponseOptions
{
    /// <summary>
    /// 自定义状态码和响应消息映射
    /// </summary>
    public Dictionary<int, string> CustomMessages { get; set; } = new();

    /// <summary>
    /// 添加自定义状态码和响应消息
    /// </summary>
    /// <param name="code">状态码</param>
    /// <param name="message">响应消息</param>
    /// <returns>当前配置对象</returns>
    public ResponseOptions AddMessage(int code, string message)
    {
        CustomMessages[code] = message;
        return this;
    }

    /// <summary>
    /// 批量添加自定义状态码和响应消息
    /// </summary>
    /// <param name="messages">状态码和响应消息的字典</param>
    /// <returns>当前配置对象</returns>
    public ResponseOptions AddMessages(Dictionary<int, string> messages)
    {
        foreach (var kvp in messages)
        {
            CustomMessages[kvp.Key] = kvp.Value;
        }
        return this;
    }
}
