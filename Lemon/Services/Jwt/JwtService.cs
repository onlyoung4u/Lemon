using System.Security.Claims;
using System.Text;
using Lemon.Models;
using Lemon.Services.Cache;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Lemon.Services.Jwt;

public class JwtService(
    IOptionsMonitor<List<JwtOptions>> jwtOptionsMonitor,
    IHybridCacheService cache,
    IFreeSql freeSql
) : IJwtService
{
    private readonly IOptionsMonitor<List<JwtOptions>> _jwtOptionsMonitor = jwtOptionsMonitor;
    private readonly IHybridCacheService _cache = cache;
    private readonly IFreeSql _freeSql = freeSql;
    private readonly JsonWebTokenHandler _tokenHandler = new();

    /// <summary>
    /// 生成JWT令牌
    /// </summary>
    public async Task<string> GenerateToken(IJwtUserInfo userInfo, string? name = null)
    {
        var options = GetJwtOptions(name);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var jti = Guid.NewGuid().ToString();

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userInfo.UserId),
            new(JwtRegisteredClaimNames.Jti, jti),
            new(
                JwtRegisteredClaimNames.Iat,
                DateTimeOffset.Now.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64
            ),
        };

        if (!string.IsNullOrEmpty(userInfo.Username))
        {
            claims.Add(new Claim("username", userInfo.Username));
        }

        if (!string.IsNullOrEmpty(userInfo.Nickname))
        {
            claims.Add(new Claim("nickname", userInfo.Nickname));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.Now.AddMinutes(options.ExpiresInMinutes),
            Issuer = options.Issuer,
            Audience = options.Audience,
            SigningCredentials = credentials,
        };

        var token = _tokenHandler.CreateToken(tokenDescriptor);

        if (options.SSO)
        {
            var cacheKey = $"lemon:jwt:{options.Name}:{userInfo.UserId}";
            await _cache.SetAsync(cacheKey, jti, TimeSpan.FromMinutes(options.ExpiresInMinutes));
        }

        return token;
    }

    /// <summary>
    /// 生成JWT令牌
    /// </summary>
    public async Task<string> GenerateToken(string userId, string? name = null)
    {
        var options = GetJwtOptions(name);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var jti = Guid.NewGuid().ToString();

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId),
            new(JwtRegisteredClaimNames.Jti, jti),
            new(
                JwtRegisteredClaimNames.Iat,
                DateTimeOffset.Now.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64
            ),
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.Now.AddMinutes(options.ExpiresInMinutes),
            Issuer = options.Issuer,
            Audience = options.Audience,
            SigningCredentials = credentials,
        };

        var token = _tokenHandler.CreateToken(tokenDescriptor);

        if (options.SSO)
        {
            var cacheKey = $"lemon:jwt:{options.Name}:{userId}";
            await _cache.SetAsync(cacheKey, jti, TimeSpan.FromMinutes(options.ExpiresInMinutes));
        }

        return token;
    }

    /// <summary>
    /// 验证JWT令牌并返回用户信息
    /// </summary>
    public async Task<IJwtUserInfo?> ValidateTokenAndGetUserInfo(string token, string? name = null)
    {
        try
        {
            var options = GetJwtOptions(name);
            var validationParameters = GetTokenValidationParameters(options);
            var result = await _tokenHandler.ValidateTokenAsync(token, validationParameters);

            if (!result.IsValid)
            {
                return null;
            }

            var jti = result.ClaimsIdentity.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
            var userId = result.ClaimsIdentity.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            var username = result.ClaimsIdentity.FindFirst("username")?.Value;
            var nickname = result.ClaimsIdentity.FindFirst("nickname")?.Value;

            if (string.IsNullOrEmpty(jti) || string.IsNullOrEmpty(userId))
            {
                return null;
            }

            var revokedKey = $"lemon:jwt:revoked:{options.Name}:{jti}";
            if (await _cache.ExistsAsync(revokedKey))
            {
                return null;
            }

            if (options.SSO)
            {
                var cacheKey = $"lemon:jwt:{options.Name}:{userId}";
                var cachedJti = await _cache.GetAsync<string>(cacheKey);
                if (cachedJti != jti)
                {
                    return null;
                }
            }

            var isInactive = await IsInactiveUser(userId, options.Name);

            if (isInactive)
            {
                return null;
            }

            return new JwtUserInfo(userId, username, nickname);
        }
        catch
        {
            return null;
        }
    }

    private async Task<bool> IsInactiveUser(string userId, string name)
    {
        name = name.ToLower();

        var cacheKey = $"lemon:user:inactive:{name}";

        var inactiveUsers = await _cache.GetFromMemoryCacheAsync<List<int>>(cacheKey);

        if (inactiveUsers == null)
        {
            inactiveUsers = await _freeSql
                .Select<LemonInactiveUser>()
                .Where(x => x.Group == name)
                .ToListAsync(x => x.UserId);

            await _cache.SetMemoryCacheAsync(cacheKey, inactiveUsers, TimeSpan.FromMinutes(10));
        }

        return inactiveUsers?.Contains(int.Parse(userId)) ?? false;
    }

    /// <summary>
    /// 撤销JWT令牌
    /// </summary>
    public async Task<bool> RevokeToken(string token, string? name = null)
    {
        try
        {
            var options = GetJwtOptions(name);
            var validationParameters = GetTokenValidationParameters(options);
            var result = await _tokenHandler.ValidateTokenAsync(token, validationParameters);

            if (!result.IsValid)
            {
                return false;
            }

            var jti = result.ClaimsIdentity.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
            var exp = result.ClaimsIdentity.FindFirst(JwtRegisteredClaimNames.Exp)?.Value;

            if (!string.IsNullOrEmpty(jti) && !string.IsNullOrEmpty(exp))
            {
                var expTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(exp));
                var revokedKey = $"lemon:jwt:revoked:{options.Name}:{jti}";
                var ttl = expTime - DateTimeOffset.Now;

                if (ttl > TimeSpan.Zero)
                {
                    await _cache.SetAsync(revokedKey, true, ttl);
                }
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 获取JWT配置选项
    /// </summary>
    private JwtOptions GetJwtOptions(string? name = null)
    {
        var allOptions = _jwtOptionsMonitor.CurrentValue;

        if (allOptions == null || allOptions.Count == 0)
        {
            throw new InvalidOperationException("未配置JWT选项");
        }

        if (string.IsNullOrEmpty(name))
        {
            return allOptions.First();
        }

        var option =
            allOptions.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            ?? throw new ArgumentException($"未找到名为 '{name}' 的JWT配置");

        return option;
    }

    /// <summary>
    /// 获取Token验证参数
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    private static TokenValidationParameters GetTokenValidationParameters(JwtOptions options)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SecretKey));

        return new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = options.Issuer,
            ValidAudience = options.Audience,
            IssuerSigningKey = key,
            ClockSkew = TimeSpan.Zero,
        };
    }
}
