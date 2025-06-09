namespace Lemon.Services.Response;

/// <summary>
/// 响应构建器接口
/// </summary>
public interface IResponseBuilder
{
    /// <summary>
    /// 创建响应
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <param name="code">状态码</param>
    /// <param name="data">数据</param>
    /// <param name="message">自定义消息</param>
    /// <returns>API响应</returns>
    ApiResponse<T> Create<T>(int code, T? data = default, string? message = null);

    /// <summary>
    /// 创建响应
    /// </summary>
    /// <param name="code">状态码</param>
    /// <param name="data">数据</param>
    /// <param name="message">自定义消息</param>
    /// <returns>API响应</returns>
    ApiResponse Create(int code, object? data = null, string? message = null);

    /// <summary>
    /// 成功响应
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <param name="data">数据</param>
    /// <param name="message">自定义消息</param>
    /// <returns>API响应</returns>
    ApiResponse<T> Success<T>(T? data = default, string? message = null);

    /// <summary>
    /// 成功响应
    /// </summary>
    /// <param name="data">数据</param>
    /// <param name="message">自定义消息</param>
    /// <returns>API响应</returns>
    ApiResponse Success(object? data = null, string? message = null);

    /// <summary>
    /// 错误响应
    /// </summary>
    /// <param name="message">自定义消息</param>
    /// <returns>API响应</returns>
    ApiResponse Error(string? message = null);

    /// <summary>
    /// 参数错误响应
    /// </summary>
    /// <param name="message">自定义消息</param>
    /// <returns>API响应</returns>
    ApiResponse BadRequest(string? message = null);

    /// <summary>
    /// 未登录或登录过期响应
    /// </summary>
    /// <param name="message">自定义消息</param>
    /// <returns>API响应</returns>
    ApiResponse Unauthorized(string? message = null);

    /// <summary>
    /// 无权限响应
    /// </summary>
    /// <param name="message">自定义消息</param>
    /// <returns>API响应</returns>
    ApiResponse Forbidden(string? message = null);
}
