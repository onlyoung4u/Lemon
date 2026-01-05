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
        // 检查是否是可选认证
        var endpoint = context.GetEndpoint();
        var optionalAuth = endpoint?.Metadata.GetMetadata<OptionalJwtAuthAttribute>();

        if (optionalAuth != null)
        {
            await TryOptionalAuthenticationAsync(context, optionalAuth.JwtName);
            await _next(context);
            return;
        }

        if (ShouldSkipAuthentication(context))
        {
            await _next(context);
            return;
        }

        // 获取 RequireJwtAuthAttribute 中指定的 JwtName
        var requireAuth = endpoint?.Metadata.GetMetadata<RequireJwtAuthAttribute>();
        var jwtService = context.RequestServices.GetRequiredService<IJwtService>();

        try
        {
            var token = ExtractTokenFromRequest(context);

            if (string.IsNullOrEmpty(token))
            {
                throw new UnauthorizedException();
            }

            // 优先使用 Attribute 中指定的 JwtName，否则从路径提取
            var jwtName = requireAuth?.JwtName;
            if (string.IsNullOrEmpty(jwtName))
            {
                var path = context.Request.Path.Value;
                if (string.IsNullOrEmpty(path))
                {
                    throw new UnauthorizedException();
                }
                var pathParts = path.Split('/').Where(x => !string.IsNullOrEmpty(x));
                jwtName = pathParts.First();
            }

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
    /// 尝试可选认证：有token则尝试获取用户信息，无token或获取失败则静默继续
    /// </summary>
    private async Task TryOptionalAuthenticationAsync(HttpContext context, string? specifiedJwtName)
    {
        try
        {
            var token = ExtractTokenFromRequest(context);

            // 没有token，直接返回
            if (string.IsNullOrEmpty(token))
            {
                return;
            }

            // 优先使用 Attribute 中指定的 JwtName，否则从路径提取
            var jwtName = specifiedJwtName;
            if (string.IsNullOrEmpty(jwtName))
            {
                var path = context.Request.Path.Value;
                if (string.IsNullOrEmpty(path))
                {
                    return;
                }
                var pathParts = path.Split('/').Where(x => !string.IsNullOrEmpty(x));
                jwtName = pathParts.First();
            }

            var jwtService = context.RequestServices.GetRequiredService<IJwtService>();
            var userInfo = await jwtService.ValidateTokenAndGetUserInfo(token, jwtName);

            // 成功获取用户信息则设置到context中
            if (userInfo != null && int.TryParse(userInfo.UserId, out var userId) && userId > 0)
            {
                context.Items["UserId"] = userId;
                context.Items["Username"] = userInfo.Username;
                context.Items["Nickname"] = userInfo.Nickname;
            }
            // 获取不到用户信息，静默继续（不抛异常）
        }
        catch (Exception ex)
        {
            // 可选认证失败时只记录日志，不抛异常
            _logger.LogDebug(ex, "可选JWT认证未能获取用户信息，继续处理请求");
        }
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

/// <summary>
/// 可选JWT认证的标记属性
/// 有token则尝试获取用户信息，无token或获取失败则静默继续
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class OptionalJwtAuthAttribute(string? jwtName = null) : Attribute
{
    public string? JwtName { get; } = jwtName;
}
