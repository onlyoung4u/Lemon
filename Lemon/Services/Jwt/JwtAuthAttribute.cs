using Lemon.Services.Exceptions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lemon.Services.Jwt;

/// <summary>
/// JWT 认证属性
/// </summary>
/// <remarks>
/// 初始化JWT认证属性
/// </remarks>
/// <param name="jwtName">JWT配置名称，默认为null（使用第一个配置）</param>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class JwtAuthAttribute(string? jwtName = null) : Attribute, IAsyncActionFilter
{
    /// <summary>
    /// JWT配置名称，默认为null（使用第一个配置）
    /// </summary>
    public string? JwtName { get; } = jwtName;

    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next
    )
    {
        var jwtService = context.HttpContext.RequestServices.GetRequiredService<IJwtService>();
        var logger = context.HttpContext.RequestServices.GetRequiredService<
            ILogger<JwtAuthAttribute>
        >();

        try
        {
            var token = ExtractTokenFromRequest(context.HttpContext);

            if (!string.IsNullOrEmpty(token))
            {
                var userId = await jwtService.ValidateToken(token, JwtName);

                if (userId > 0)
                {
                    context.HttpContext.Items["UserId"] = userId.ToString();

                    await next();

                    return;
                }
                else
                {
                    throw new UnauthorizedException();
                }
            }
            else
            {
                throw new UnauthorizedException("未提供 token");
            }
        }
        catch (UnauthorizedException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "JWT认证过程中发生异常");
            throw new UnauthorizedException();
        }
    }

    /// <summary>
    /// 从请求中提取JWT token
    /// </summary>
    /// <param name="context">HTTP上下文</param>
    /// <returns>JWT token</returns>
    private static string? ExtractTokenFromRequest(Microsoft.AspNetCore.Http.HttpContext context)
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
