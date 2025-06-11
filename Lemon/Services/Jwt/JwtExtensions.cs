using Microsoft.AspNetCore.Http;

namespace Lemon.Services.Jwt;

/// <summary>
/// JWT扩展方法
/// </summary>
public static class JwtExtensions
{
    /// <summary>
    /// 获取当前认证用户的ID
    /// </summary>
    /// <param name="context">HTTP上下文</param>
    /// <returns>用户ID</returns>
    public static string? GetUserId(this HttpContext context)
    {
        return context.Items["UserId"] as string;
    }
}
