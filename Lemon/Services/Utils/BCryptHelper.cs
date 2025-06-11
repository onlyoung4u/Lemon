namespace Lemon.Services.Utils;

/// <summary>
/// BCrypt 密码加密工具类
/// </summary>
public static class BCryptHelper
{
    /// <summary>
    /// 对密码进行 BCrypt 加密
    /// </summary>
    /// <param name="password">原始密码</param>
    /// <param name="workFactor">工作因子，默认为 12</param>
    /// <returns>加密后的哈希值</returns>
    public static string HashPassword(string password, int workFactor = 12)
    {
        if (string.IsNullOrEmpty(password))
        {
            throw new ArgumentException("密码不能为空", nameof(password));
        }

        return BCrypt.Net.BCrypt.HashPassword(password, workFactor);
    }

    /// <summary>
    /// 验证密码是否与哈希值匹配
    /// </summary>
    /// <param name="password">原始密码</param>
    /// <param name="hashedPassword">已哈希的密码</param>
    /// <returns>密码是否匹配</returns>
    public static bool VerifyPassword(string password, string hashedPassword)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hashedPassword))
        {
            return false;
        }

        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
        catch
        {
            return false;
        }
    }
}
