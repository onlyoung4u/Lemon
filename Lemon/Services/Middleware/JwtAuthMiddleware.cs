using Lemon.Services.Exceptions;
using Lemon.Services.Jwt;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lemon.Services.Middleware;

/// <summary>
/// JWT 认证中间件
/// </summary>
public class JwtAuthMiddleware(RequestDelegate next, ILogger<JwtAuthMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<JwtAuthMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        if (ShouldSkipAuthentication(context))
        {
            await _next(context);
            return;
        }

        var jwtService = context.RequestServices.GetRequiredService<IJwtService>();

        try
        {
            var token = ExtractTokenFromRequest(context);
            var path = context.Request.Path.Value;

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(path))
            {
                throw new UnauthorizedException();
            }

            var pathParts = path.Split('/').Where(x => !string.IsNullOrEmpty(x));
            var jwtName = pathParts.First();

            var userInfo = await jwtService.ValidateTokenAndGetUserInfo(token, jwtName);

            if (userInfo != null && int.TryParse(userInfo.UserId, out var userId) && userId > 0)
            {
                context.Items["UserId"] = userId;
                context.Items["Username"] = userInfo.Username;
                context.Items["Nickname"] = userInfo.Nickname;
            }
            else
            {
                throw new UnauthorizedException();
            }
        }
        catch (UnauthorizedException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "JWT认证过程中发生异常");
            throw new UnauthorizedException();
        }

        await _next(context);
    }

    /// <summary>
    /// 判断是否应该跳过认证
    /// </summary>
    private static bool ShouldSkipAuthentication(HttpContext context)
    {
        // 获取端点信息
        var endpoint = context.GetEndpoint();
        if (endpoint == null)
            return true;

        // 检查是否有跳过认证的标记
        var skipAuth = endpoint.Metadata.GetMetadata<SkipJwtAuthAttribute>();
        if (skipAuth != null)
            return true;

        // 检查是否需要认证
        var requireAuth = endpoint.Metadata.GetMetadata<RequireJwtAuthAttribute>();
        return requireAuth == null;
    }

    /// <summary>
    /// 从请求中提取JWT token
    /// </summary>
    private static string? ExtractTokenFromRequest(HttpContext context)
    {
        var authorizationHeader = context.Request.Headers.Authorization.FirstOrDefault();

        if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith("Bearer "))
        {
            return authorizationHeader["Bearer ".Length..].Trim();
        }

        if (context.Request.Query.TryGetValue("token", out var tokenValue))
        {
            return tokenValue.FirstOrDefault();
        }

        if (context.Request.Cookies.TryGetValue("jwt-token", out var cookieToken))
        {
            return cookieToken;
        }

        return null;
    }
}

/// <summary>
/// 需要JWT认证的标记属性
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireJwtAuthAttribute(string? jwtName = null) : Attribute
{
    public string? JwtName { get; } = jwtName;
}

/// <summary>
/// 跳过JWT认证的标记属性
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class SkipJwtAuthAttribute : Attribute { }
