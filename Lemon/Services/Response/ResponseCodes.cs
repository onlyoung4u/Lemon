namespace Lemon.Services.Response;

/// <summary>
/// 响应状态码常量
/// </summary>
public static class ResponseCodes
{
    /// <summary>
    /// 操作成功
    /// </summary>
    public const int Success = 0;

    /// <summary>
    /// 操作失败
    /// </summary>
    public const int Failure = 1;

    /// <summary>
    /// 参数错误
    /// </summary>
    public const int BadRequest = 2;

    /// <summary>
    /// 未登录或登录过期
    /// </summary>
    public const int Unauthorized = 1000;

    /// <summary>
    /// 无权限
    /// </summary>
    public const int Forbidden = 1001;
}
