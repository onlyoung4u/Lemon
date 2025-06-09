namespace Lemon.Services.Response;

/// <summary>
/// 响应构建器实现
/// </summary>
public class ResponseBuilder(IResponseMessageService messageService) : IResponseBuilder
{
    private readonly IResponseMessageService _messageService = messageService;

    /// <summary>
    /// 创建响应
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <param name="code">状态码</param>
    /// <param name="data">数据</param>
    /// <param name="message">自定义消息</param>
    /// <returns>API响应</returns>
    public ApiResponse<T> Create<T>(int code, T? data = default, string? message = null)
    {
        var responseMessage = message ?? _messageService.GetMessage(code);
        return new ApiResponse<T>(code, responseMessage, data);
    }

    /// <summary>
    /// 创建响应
    /// </summary>
    /// <param name="code">状态码</param>
    /// <param name="data">数据</param>
    /// <param name="message">自定义消息</param>
    /// <returns>API响应</returns>
    public ApiResponse Create(int code, object? data = null, string? message = null)
    {
        var responseMessage = message ?? _messageService.GetMessage(code);
        return new ApiResponse(code, responseMessage, data);
    }

    /// <summary>
    /// 成功响应
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <param name="data">数据</param>
    /// <param name="message">自定义消息</param>
    /// <returns>API响应</returns>
    public ApiResponse<T> Success<T>(T? data = default, string? message = null)
    {
        return Create(ResponseCodes.Success, data, message);
    }

    /// <summary>
    /// 成功响应
    /// </summary>
    /// <param name="data">数据</param>
    /// <param name="message">自定义消息</param>
    /// <returns>API响应</returns>
    public ApiResponse Success(object? data = null, string? message = null)
    {
        return Create(ResponseCodes.Success, data, message);
    }

    /// <summary>
    /// 错误响应
    /// </summary>
    /// <param name="message">自定义消息</param>
    /// <returns>API响应</returns>
    public ApiResponse Error(string? message = null)
    {
        return Create(ResponseCodes.Failure, null, message);
    }

    /// <summary>
    /// 参数错误响应
    /// </summary>
    /// <param name="message">自定义消息</param>
    /// <returns>API响应</returns>
    public ApiResponse BadRequest(string? message = null)
    {
        return Create(ResponseCodes.BadRequest, null, message);
    }

    /// <summary>
    /// 未登录或登录过期响应
    /// </summary>
    /// <param name="message">自定义消息</param>
    /// <returns>API响应</returns>
    public ApiResponse Unauthorized(string? message = null)
    {
        return Create(ResponseCodes.Unauthorized, null, message);
    }

    /// <summary>
    /// 无权限响应
    /// </summary>
    /// <param name="message">自定义消息</param>
    /// <returns>API响应</returns>
    public ApiResponse Forbidden(string? message = null)
    {
        return Create(ResponseCodes.Forbidden, null, message);
    }
}
