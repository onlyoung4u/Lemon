using Lemon.Services.Response;

namespace Lemon.Services.Exceptions;

/// <summary>
/// Lemon 框架基础异常类
/// </summary>
public class LemonException(int code = ResponseCodes.Failure, string? message = null)
    : Exception(message)
{
    /// <summary>
    /// 错误代码
    /// </summary>
    public int Code { get; } = code;

    /// <summary>
    /// 错误消息
    /// </summary>
    public new string? Message { get; } = message;
}

/// <summary>
/// 参数错误异常
/// </summary>
public class BadRequestException(string? message = null)
    : LemonException(ResponseCodes.BadRequest, message) { }

/// <summary>
/// 未登录异常
/// </summary>
public class UnauthorizedException(string? message = null)
    : LemonException(ResponseCodes.Unauthorized, message) { }

/// <summary>
/// 无权限异常
/// </summary>
public class ForbiddenException(string? message = null)
    : LemonException(ResponseCodes.Forbidden, message) { }

/// <summary>
/// 业务异常
/// </summary>
public class BusinessException(string? message = null)
    : LemonException(ResponseCodes.Failure, message) { }
