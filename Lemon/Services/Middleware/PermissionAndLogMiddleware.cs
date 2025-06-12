using System.Reflection;
using System.Text;
using Lemon.Models;
using Lemon.Services.Attributes;
using Lemon.Services.Exceptions;
using Lemon.Services.Jwt;
using Lemon.Services.Permission;
using Lemon.Services.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lemon.Services.Middleware;

/// <summary>
/// 权限检查和日志记录中间件
/// </summary>
public class PermissionAndLogMiddleware(
    RequestDelegate next,
    ILogger<PermissionAndLogMiddleware> logger
)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<PermissionAndLogMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        // 获取端点信息
        var endpoint = context.GetEndpoint();
        var attribute = endpoint?.Metadata.GetMetadata<PermissionAndLogAttribute>();

        // 如果没有PermissionAndLogAttribute，直接执行下一个中间件
        if (attribute == null)
        {
            await _next(context);
            return;
        }

        // 读取请求体并缓存（在执行下一个中间件之前）
        string requestBody = string.Empty;
        if (context.Request.ContentLength > 0 || context.Request.ContentLength == null)
        {
            requestBody = await ReadRequestBodyAsync(context.Request);
            context.Items["RequestBody"] = requestBody;
        }

        // 权限检查
        if (!string.IsNullOrEmpty(attribute.Permission))
        {
            await CheckPermissionAsync(context, attribute.Permission);
        }

        // 执行下一个中间件
        await _next(context);

        // 记录日志（在响应之后）
        if (!string.IsNullOrEmpty(attribute.Description))
        {
            await LogOperationAsync(context, attribute.Description, requestBody);
        }
    }

    /// <summary>
    /// 权限检查
    /// </summary>
    private static async Task CheckPermissionAsync(HttpContext context, string permission)
    {
        var userId = context.GetUserId();

        if (userId == 0)
        {
            throw new ForbiddenException();
        }

        var permissionService =
            context.RequestServices.GetService<IPermissionService>()
            ?? throw new ForbiddenException();

        var hasPermission = await permissionService.CheckPermissionAsync(userId, permission);

        if (!hasPermission)
        {
            throw new ForbiddenException();
        }
    }

    /// <summary>
    /// 记录操作日志
    /// </summary>
    private async Task LogOperationAsync(
        HttpContext context,
        string description,
        string requestBody
    )
    {
        try
        {
            var userId = context.GetUserId();
            var username = context.GetUsername() ?? string.Empty;
            var nickname = context.GetNickname() ?? string.Empty;

            var freeSql = context.RequestServices.GetService<IFreeSql>();
            if (freeSql == null)
            {
                return;
            }

            var responseHeaders = context.Response.Headers;
            var isError = responseHeaders.ContainsKey("X-Lemon-Error");

            var log = new LemonOperationLog
            {
                UserId = userId,
                Username = username,
                Nickname = nickname,
                Path = context.Request.Path,
                Description = description,
                Method = context.Request.Method,
                Ip = IpHelper.GetClientIpAddress(context),
                Body = requestBody,
                Success = !isError,
            };

            await freeSql.Insert(log).ExecuteAffrowsAsync();

            // 调试日志
            _logger.LogInformation(
                "记录操作日志 - 用户: {Username}, 操作: {Description}, 请求体长度: {BodyLength}",
                username,
                description,
                requestBody?.Length ?? 0
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "操作日志记录失败");
        }
    }

    /// <summary>
    /// 读取请求体内容
    /// </summary>
    private static async Task<string> ReadRequestBodyAsync(HttpRequest request)
    {
        try
        {
            if (request.Body == null)
            {
                return string.Empty;
            }

            // 启用缓冲，允许多次读取流
            request.EnableBuffering();

            // 确保流位置在开始
            request.Body.Position = 0;

            // 读取请求体
            using var memoryStream = new MemoryStream();
            await request.Body.CopyToAsync(memoryStream);

            // 重置流位置，供后续使用
            request.Body.Position = 0;

            var bodyBytes = memoryStream.ToArray();

            if (bodyBytes.Length == 0)
            {
                return string.Empty;
            }

            var body = Encoding.UTF8.GetString(bodyBytes);

            return body;
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }
}
