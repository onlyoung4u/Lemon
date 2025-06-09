namespace Lemon.Services.Jwt;

public interface IJwtService
{
    /// <summary>
    /// 生成JWT令牌
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    Task<string> GenerateToken(string userId, string? name = null);

    /// <summary>
    /// 验证JWT令牌
    /// </summary>
    /// <param name="token"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    Task<int> ValidateToken(string token, string? name = null);

    /// <summary>
    /// 撤销JWT令牌
    /// </summary>
    /// <param name="token"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    Task<bool> RevokeToken(string token, string? name = null);
}
