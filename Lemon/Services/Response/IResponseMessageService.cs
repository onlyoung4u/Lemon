namespace Lemon.Services.Response;

/// <summary>
/// 响应消息服务接口
/// </summary>
public interface IResponseMessageService
{
    /// <summary>
    /// 根据状态码获取对应的响应消息
    /// </summary>
    /// <param name="code">状态码</param>
    /// <returns>响应消息</returns>
    string GetMessage(int code);

    /// <summary>
    /// 添加或更新状态码对应的响应消息
    /// </summary>
    /// <param name="code">状态码</param>
    /// <param name="message">响应消息</param>
    void SetMessage(int code, string message);

    /// <summary>
    /// 批量设置状态码和响应消息
    /// </summary>
    /// <param name="messages">状态码和响应消息的字典</param>
    void SetMessages(Dictionary<int, string> messages);
}
