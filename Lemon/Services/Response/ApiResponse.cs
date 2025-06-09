namespace Lemon.Services.Response;

/// <summary>
/// 统一API响应模型
/// </summary>
/// <typeparam name="T">响应数据类型</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// 状态码
    /// </summary>
    public int Code { get; set; }

    /// <summary>
    /// 响应消息
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 响应数据
    /// </summary>
    public T? Data { get; set; }

    public ApiResponse() { }

    public ApiResponse(int code, string message, T? data = default)
    {
        Code = code;
        Message = message;
        Data = data;
    }
}

/// <summary>
/// 无泛型的统一API响应模型
/// </summary>
public class ApiResponse : ApiResponse<object>
{
    public ApiResponse() { }

    public ApiResponse(int code, string message, object? data = null)
        : base(code, message, data) { }
}
