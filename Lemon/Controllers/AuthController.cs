using Lemon.Dtos;
using Lemon.Models;
using Lemon.Services.Attributes;
using Lemon.Services.Jwt;
using Lemon.Services.Middleware;
using Lemon.Services.Response;
using Lemon.Services.Utils;
using Microsoft.AspNetCore.Mvc;

namespace Lemon.Controllers;

[ApiController]
[Route("admin/auth")]
public class AuthController(
    IResponseBuilder responseBuilder,
    IFreeSql freeSql,
    IJwtService jwtService
) : LemonController(responseBuilder)
{
    private readonly IFreeSql _freeSql = freeSql;
    private readonly IJwtService _jwtService = jwtService;

    /// <summary>
    /// 登录
    /// </summary>
    [HttpPost("login")]
    [LemonAdmin(null, "登录")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _freeSql
            .Select<LemonUser>()
            .Where(x => x.Username == request.Username)
            .Where(x => x.DeletedAt == null)
            .ToOneAsync();

        if (user == null || !BCryptHelper.VerifyPassword(request.Password, user.Password))
        {
            return Error("用户名或密码错误");
        }

        if (!user.IsActive)
        {
            return Error("用户已禁用");
        }

        var userInfo = new JwtUserInfo(user.Id.ToString(), user.Username, user.Nickname);
        var token = await _jwtService.GenerateToken(userInfo);

        await _freeSql
            .Update<LemonUser>(user.Id)
            .Set(x => x.LastLoginIp, IpHelper.GetClientIpAddress(HttpContext))
            .Set(x => x.LastLoginTime, DateTime.UtcNow)
            .ExecuteAffrowsAsync();

        return Success(new { token });
    }

    /// <summary>
    /// 退出登录
    /// </summary>
    [HttpPost("logout")]
    [RequireJwtAuth]
    [LemonAdmin(null, "退出登录")]
    public async Task<IActionResult> Logout(
        [FromHeader(Name = "Authorization")] string? authorization
    )
    {
        if (string.IsNullOrEmpty(authorization) || !authorization.StartsWith("Bearer "))
        {
            return Error();
        }

        await _jwtService.RevokeToken(authorization["Bearer ".Length..].Trim());

        return Success();
    }
}
