using FluentValidation;
using Lemon.Business.System;
using Lemon.Dtos;
using Lemon.Services.Attributes;
using Lemon.Services.Middleware;
using Lemon.Services.Response;
using Microsoft.AspNetCore.Mvc;

namespace Lemon.Controllers;

[ApiController]
[Route("admin/system/menu")]
[RequireJwtAuth]
public class MenuController(IResponseBuilder responseBuilder, IMenuService menuService)
    : LemonController(responseBuilder)
{
    private readonly IMenuService _menuService = menuService;

    [HttpGet]
    [LemonAdmin("menu.list")]
    public async Task<IActionResult> GetMenus(bool withButton = false)
    {
        var menus = await _menuService.GetMenusAsync(withButton);

        return Success(menus);
    }

    [HttpPost]
    [LemonAdmin("menu.create", "添加菜单")]
    public async Task<IActionResult> CreateMenu(
        [FromBody] MenuCreateRequest request,
        IValidator<MenuCreateRequest> validator
    )
    {
        await ValidateAsync(request, validator);

        await _menuService.CreateMenuAsync(request);

        return Success();
    }

    [HttpPut("{id:int:min(1)}")]
    [LemonAdmin("menu.update", "编辑菜单")]
    public async Task<IActionResult> UpdateMenu(
        int id,
        [FromBody] MenuUpdateRequest request,
        IValidator<MenuUpdateRequest> validator
    )
    {
        await ValidateAsync(request, validator);

        await _menuService.UpdateMenuAsync(id, request);

        return Success();
    }

    [HttpDelete("{id:int:min(1)}")]
    [LemonAdmin("menu.delete", "删除菜单")]
    public async Task<IActionResult> DeleteMenu(int id)
    {
        await _menuService.DeleteMenuAsync(id);

        return Success();
    }
}
