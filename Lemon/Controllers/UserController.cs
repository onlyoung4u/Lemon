using FluentValidation;
using Lemon.Business.System;
using Lemon.Dtos;
using Lemon.Services.Attributes;
using Lemon.Services.Extensions;
using Lemon.Services.Middleware;
using Lemon.Services.Response;
using Microsoft.AspNetCore.Mvc;

namespace Lemon.Controllers;

[ApiController]
[Route("admin/system/user")]
[RequireJwtAuth]
public class UserController(IResponseBuilder responseBuilder, IUserService userService)
    : LemonController(responseBuilder)
{
    private readonly IUserService _userService = userService;

    /// <summary>
    /// 获取用户列表
    /// </summary>
    /// <param name="request">查询参数</param>
    /// <param name="validator">验证器</param>
    /// <returns></returns>
    [HttpGet]
    [LemonAdmin("user.list")]
    public async Task<IActionResult> GetUsers(
        [FromQuery] UserQueryRequest request,
        IValidator<UserQueryRequest> validator
    )
    {
        await ValidateAsync(request, validator);

        var result = await _userService.GetUsersAsync(request, HttpContext.GetUserId());

        return Success(result);
    }

    /// <summary>
    /// 创建用户
    /// </summary>
    /// <param name="request">创建用户请求</param>
    /// <param name="validator">验证器</param>
    /// <returns></returns>
    [HttpPost]
    [LemonAdmin("user.create", "添加用户")]
    public async Task<IActionResult> CreateUser(
        [FromBody] UserCreateRequest request,
        IValidator<UserCreateRequest> validator
    )
    {
        await ValidateAsync(request, validator);

        await _userService.CreateUserAsync(request, HttpContext.GetUserId());

        return Success();
    }

    /// <summary>
    /// 更新用户
    /// </summary>
    /// <param name="id">用户ID</param>
    /// <param name="request">更新用户请求</param>
    /// <param name="validator">验证器</param>
    /// <returns></returns>
    [HttpPut("{id:int:min(1)}")]
    [LemonAdmin("user.update", "编辑用户")]
    public async Task<IActionResult> UpdateUser(
        int id,
        [FromBody] UserUpdateRequest request,
        IValidator<UserUpdateRequest> validator
    )
    {
        await ValidateAsync(request, validator);

        await _userService.UpdateUserAsync(id, request, HttpContext.GetUserId());

        return Success();
    }

    /// <summary>
    /// 删除用户
    /// </summary>
    /// <param name="id">用户ID</param>
    /// <returns></returns>
    [HttpDelete("{id:int:min(1)}")]
    [LemonAdmin("user.delete", "删除用户")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        await _userService.DeleteUserAsync(id, HttpContext.GetUserId());

        return Success();
    }

    /// <summary>
    /// 设置用户状态
    /// </summary>
    /// <param name="id">用户ID</param>
    /// <param name="isActive">是否激活</param>
    /// <returns></returns>
    [HttpPut("{id:int:min(1)}/active")]
    [LemonAdmin("user.update", "设置用户状态")]
    public async Task<IActionResult> SetUserActive(
        int id,
        [FromBody] UserActiveRequest request,
        IValidator<UserActiveRequest> validator
    )
    {
        await ValidateAsync(request, validator);

        await _userService.SetUserActiveAsync(
            id,
            request.IsActive ?? true,
            HttpContext.GetUserId()
        );

        return Success();
    }
}
