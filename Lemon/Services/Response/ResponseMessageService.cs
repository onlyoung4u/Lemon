using System.Collections.Concurrent;

namespace Lemon.Services.Response;

/// <summary>
/// 响应消息服务实现
/// </summary>
public class ResponseMessageService : IResponseMessageService
{
    private readonly ConcurrentDictionary<int, string> _messages = new();

    public ResponseMessageService()
    {
        // 初始化内置的状态码和响应消息
        InitializeDefaultMessages();
    }

    /// <summary>
    /// 根据状态码获取对应的响应消息
    /// </summary>
    /// <param name="code">状态码</param>
    /// <returns>响应消息</returns>
    public string GetMessage(int code)
    {
        return _messages.GetValueOrDefault(code, "未知错误");
    }

    /// <summary>
    /// 添加或更新状态码对应的响应消息
    /// </summary>
    /// <param name="code">状态码</param>
    /// <param name="message">响应消息</param>
    public void SetMessage(int code, string message)
    {
        _messages.AddOrUpdate(code, message, (key, oldValue) => message);
    }

    /// <summary>
    /// 批量设置状态码和响应消息
    /// </summary>
    /// <param name="messages">状态码和响应消息的字典</param>
    public void SetMessages(Dictionary<int, string> messages)
    {
        foreach (var kvp in messages)
        {
            SetMessage(kvp.Key, kvp.Value);
        }
    }

    /// <summary>
    /// 初始化默认的状态码和响应消息
    /// </summary>
    private void InitializeDefaultMessages()
    {
        _messages[ResponseCodes.Success] = "操作成功";
        _messages[ResponseCodes.Failure] = "操作失败";
        _messages[ResponseCodes.BadRequest] = "参数错误";
        _messages[ResponseCodes.Unauthorized] = "未登录或登录过期";
        _messages[ResponseCodes.Forbidden] = "无权限";
    }
}
