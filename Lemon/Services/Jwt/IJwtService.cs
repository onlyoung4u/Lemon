namespace Lemon.Services.Jwt;

/// <summary>
/// JWT用户信息接口
/// </summary>
public interface IJwtUserInfo
{
    /// <summary>
    /// 用户ID
    /// </summary>
    string UserId { get; }

    /// <summary>
    /// 用户名
    /// </summary>
    string? Username { get; }

    /// <summary>
    /// 昵称
    /// </summary>
    string? Nickname { get; }
}

/// <summary>
/// JWT用户信息实现类
/// </summary>
public class JwtUserInfo(string userId, string? username = null, string? nickname = null)
    : IJwtUserInfo
{
    public string UserId { get; set; } = userId;
    public string? Username { get; set; } = username;
    public string? Nickname { get; set; } = nickname;
}

public interface IJwtService
{
    /// <summary>
    /// 生成JWT令牌
    /// </summary>
    /// <param name="userInfo">用户信息</param>
    /// <param name="name">JWT配置名称</param>
    /// <returns></returns>
    Task<string> GenerateToken(IJwtUserInfo userInfo, string? name = null);

    /// <summary>
    /// 生成JWT令牌
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="name">JWT配置名称</param>
    /// <returns></returns>
    Task<string> GenerateToken(string userId, string? name = null);

    /// <summary>
    /// 验证JWT令牌并返回用户信息
    /// </summary>
    /// <param name="token">JWT令牌</param>
    /// <param name="name">JWT配置名称</param>
    /// <returns></returns>
    Task<IJwtUserInfo?> ValidateTokenAndGetUserInfo(string token, string? name = null);

    /// <summary>
    /// 撤销JWT令牌
    /// </summary>
    /// <param name="token">JWT令牌</param>
    /// <param name="name">JWT配置名称</param>
    /// <returns></returns>
    Task<bool> RevokeToken(string token, string? name = null);
}
