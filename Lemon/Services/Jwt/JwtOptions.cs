namespace Lemon.Services.Jwt;

/// <summary>
/// JWT配置选项
/// </summary>
public class JwtOptions
{
    /// <summary>
    /// 配置名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 密钥
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// 发行者
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// 接收者
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// 过期时间（分钟）
    /// </summary>
    public int ExpiresInMinutes { get; set; } = 1440;

    /// <summary>
    /// 是否单点登录
    /// </summary>
    public bool SSO { get; set; } = false;
}
