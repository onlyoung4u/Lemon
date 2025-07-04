using FluentValidation;
using Lemon.Business.Auth;
using Lemon.Dtos;
using Lemon.Services.Attributes;
using Lemon.Services.Extensions;
using Lemon.Services.Middleware;
using Lemon.Services.Response;
using Lemon.Services.Utils;
using Microsoft.AspNetCore.Mvc;

namespace Lemon.Controllers;

[ApiController]
[Route("admin/auth")]
[RequireJwtAuth]
public class AuthController(IResponseBuilder responseBuilder, IAuthService authService)
    : LemonController(responseBuilder)
{
    private readonly IAuthService _authService = authService;

    /// <summary>
    /// 登录
    /// </summary>
    [HttpPost("login")]
    [SkipJwtAuth]
    [LemonAdmin(null, "登录")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        IValidator<LoginRequest> validator
    )
    {
        await ValidateAsync(request, validator);

        var result = await _authService.LoginAsync(
            request,
            IpHelper.GetClientIpAddress(HttpContext)
        );

        return Success(result);
    }

    /// <summary>
    /// 退出登录
    /// </summary>
    [HttpPost("logout")]
    [LemonAdmin(null, "退出登录")]
    public async Task<IActionResult> Logout(
        [FromHeader(Name = "Authorization")] string? authorization
    )
    {
        if (string.IsNullOrEmpty(authorization) || !authorization.StartsWith("Bearer "))
        {
            return Error();
        }

        await _authService.LogoutAsync(authorization["Bearer ".Length..].Trim());

        return Success();
    }

    /// <summary>
    /// 获取用户信息
    /// </summary>
    [HttpGet("user-info")]
    public async Task<IActionResult> GetUserInfo()
    {
        var userId = HttpContext.GetUserId();

        var result = await _authService.GetUserInfoAsync(userId);

        return Success(result);
    }

    /// <summary>
    /// 获取用户权限
    /// </summary>
    [HttpGet("permissions")]
    public async Task<IActionResult> GetPermissions()
    {
        var userId = HttpContext.GetUserId();

        var result = await _authService.GetPermissionsAsync(userId);

        return Success(result);
    }

    /// <summary>
    /// 获取用户菜单
    /// </summary>
    [HttpGet("menus")]
    public async Task<IActionResult> GetMenus()
    {
        var userId = HttpContext.GetUserId();

        var result = await _authService.GetMenusAsync(userId);

        return Success(result);
    }

    /// <summary>
    /// 修改密码
    /// </summary>
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        IValidator<ChangePasswordRequest> validator
    )
    {
        await ValidateAsync(request, validator);

        await _authService.ChangePasswordAsync(request, HttpContext.GetUserId());

        return Success();
    }
}
