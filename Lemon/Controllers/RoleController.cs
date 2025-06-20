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
[Route("admin/system/role")]
[RequireJwtAuth]
public class RoleController(IResponseBuilder responseBuilder, IRoleService roleService)
    : LemonController(responseBuilder)
{
    private readonly IRoleService _roleService = roleService;

    /// <summary>
    /// 获取角色列表
    /// </summary>
    /// <param name="request">查询参数</param>
    /// <returns></returns>
    [HttpGet]
    [LemonAdmin("role.list")]
    public async Task<IActionResult> GetRoles([FromQuery] RoleQueryRequest request)
    {
        var result = await _roleService.GetRolesAsync(request, HttpContext.GetUserId());

        return Success(result);
    }

    /// <summary>
    /// 创建角色
    /// </summary>
    /// <param name="request">创建角色请求</param>
    /// <param name="validator">验证器</param>
    /// <returns></returns>
    [HttpPost]
    [LemonAdmin("role.create", "添加角色")]
    public async Task<IActionResult> CreateRole(
        [FromBody] RoleCreateRequest request,
        IValidator<RoleCreateRequest> validator
    )
    {
        await ValidateAsync(request, validator);

        await _roleService.CreateRoleAsync(request, HttpContext.GetUserId());

        return Success();
    }

    /// <summary>
    /// 更新角色
    /// </summary>
    /// <param name="id">角色ID</param>
    /// <param name="request">更新角色请求</param>
    /// <param name="validator">验证器</param>
    /// <returns></returns>
    [HttpPut("{id:int:min(1)}")]
    [LemonAdmin("role.update", "编辑角色")]
    public async Task<IActionResult> UpdateRole(
        int id,
        [FromBody] RoleUpdateRequest request,
        IValidator<RoleUpdateRequest> validator
    )
    {
        await ValidateAsync(request, validator);

        await _roleService.UpdateRoleAsync(id, request, HttpContext.GetUserId());

        return Success();
    }

    /// <summary>
    /// 删除角色
    /// </summary>
    /// <param name="id">角色ID</param>
    /// <returns></returns>
    [HttpDelete("{id:int:min(1)}")]
    [LemonAdmin("role.delete", "删除角色")]
    public async Task<IActionResult> DeleteRole(int id)
    {
        await _roleService.DeleteRoleAsync(id, HttpContext.GetUserId());

        return Success();
    }
}
