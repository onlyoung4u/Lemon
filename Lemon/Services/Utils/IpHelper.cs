using System.Net;
using Microsoft.AspNetCore.Http;

namespace Lemon.Services.Utils;

/// <summary>
/// IP地址工具类
/// </summary>
public static class IpHelper
{
    /// <summary>
    /// 获取客户端真实IP地址
    /// </summary>
    /// <param name="httpContext">HTTP上下文</param>
    /// <returns>客户端IP地址</returns>
    public static string GetClientIpAddress(HttpContext httpContext)
    {
        // 按优先级顺序检查各种可能的IP地址来源
        var ipAddress =
            GetIpFromHeader(httpContext, "X-Forwarded-For")
            ?? GetIpFromHeader(httpContext, "X-Real-IP")
            ?? GetIpFromHeader(httpContext, "X-Client-IP")
            ?? GetIpFromHeader(httpContext, "CF-Connecting-IP")
            ?? GetIpFromHeader(httpContext, "True-Client-IP")
            ?? httpContext.Connection.RemoteIpAddress?.ToString();

        return NormalizeIpAddress(ipAddress);
    }

    /// <summary>
    /// 从请求头中获取IP地址
    /// </summary>
    /// <param name="httpContext">HTTP上下文</param>
    /// <param name="headerName">请求头名称</param>
    /// <returns>IP地址或null</returns>
    private static string? GetIpFromHeader(HttpContext httpContext, string headerName)
    {
        var headerValue = httpContext.Request.Headers[headerName].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(headerValue))
        {
            return null;
        }

        // X-Forwarded-For 可能包含多个IP地址，用逗号分隔，第一个是真实客户端IP
        var firstIp = headerValue
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault()
            ?.Trim();

        return IsValidIpAddress(firstIp) ? firstIp : null;
    }

    /// <summary>
    /// 验证IP地址格式是否有效
    /// </summary>
    /// <param name="ipAddress">IP地址字符串</param>
    /// <returns>是否有效</returns>
    private static bool IsValidIpAddress(string? ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
        {
            return false;
        }

        return IPAddress.TryParse(ipAddress, out var parsedIp) && !IsPrivateIpAddress(parsedIp);
    }

    /// <summary>
    /// 检查是否为私有IP地址
    /// </summary>
    /// <param name="ipAddress">IP地址</param>
    /// <returns>是否为私有IP</returns>
    private static bool IsPrivateIpAddress(IPAddress ipAddress)
    {
        var bytes = ipAddress.GetAddressBytes();

        // IPv4私有地址范围
        if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
        {
            // 10.0.0.0/8
            if (bytes[0] == 10)
                return true;

            // 172.16.0.0/12
            if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31)
                return true;

            // 192.168.0.0/16
            if (bytes[0] == 192 && bytes[1] == 168)
                return true;

            // 127.0.0.0/8 (localhost)
            if (bytes[0] == 127)
                return true;
        }

        return false;
    }

    /// <summary>
    /// 规范化IP地址
    /// </summary>
    /// <param name="ipAddress">IP地址字符串</param>
    /// <returns>规范化后的IP地址</returns>
    private static string NormalizeIpAddress(string? ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
        {
            return "unknown";
        }

        // 移除IPv6的前缀
        if (ipAddress.StartsWith("::ffff:"))
        {
            ipAddress = ipAddress[7..];
        }

        return ipAddress;
    }
}
