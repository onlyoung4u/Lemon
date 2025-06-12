using Microsoft.AspNetCore.Http;

namespace Lemon.Services.Extensions;

/// <summary>
/// JWT扩展方法
/// </summary>
public static class UserInfoExtensions
{
    /// <summary>
    /// 获取当前认证用户的ID
    /// </summary>
    /// <param name="context">HTTP上下文</param>
    /// <returns>用户ID</returns>
    public static int GetUserId(this HttpContext context)
    {
        return context.Items["UserId"] as int? ?? 0;
    }

    /// <summary>
    /// 获取当前认证用户的用户名
    /// </summary>
    /// <param name="context">HTTP上下文</param>
    /// <returns>用户名</returns>
    public static string? GetUsername(this HttpContext context)
    {
        return context.Items["Username"] as string;
    }

    /// <summary>
    /// 获取当前认证用户的昵称
    /// </summary>
    /// <param name="context">HTTP上下文</param>
    /// <returns>用户昵称</returns>
    public static string? GetNickname(this HttpContext context)
    {
        return context.Items["Nickname"] as string;
    }
}
