using Lemon.Business.Base;
using Lemon.Dtos;
using Lemon.Models;
using Lemon.Services.Constants;
using Lemon.Services.Exceptions;
using Lemon.Services.Permission;
using Mapster;

namespace Lemon.Business.System;

public class MenuService(IFreeSql freeSql, IPermissionService permissionService)
    : BaseService(freeSql),
        IMenuService
{
    private readonly IPermissionService _permissionService = permissionService;

    private static List<MenuResponse> HandleMenuTree(List<LemonMenu> menus, int parentId = 0)
    {
        var tree = new List<MenuResponse>();

        foreach (var menu in menus)
        {
            if (menu.ParentId == parentId)
            {
                var menuDto = menu.Adapt<MenuResponse>();

                var children = HandleMenuTree(menus, menu.Id);

                if (children.Count > 0)
                {
                    menuDto.Children = children;
                }

                tree.Add(menuDto);
            }
        }

        return tree;
    }

    public async Task<List<MenuResponse>> GetUserMenusAsync(int userId)
    {
        if (userId == 1)
        {
            return await GetMenusAsync(true);
        }

        var userPermissions = await _permissionService.GetUserPermissionsAsync(userId);

        if (userPermissions.Count == 0)
        {
            return [];
        }

        var menus = await _freeSql
            .Select<LemonMenu>()
            .WhereIf(userId > 0, x => userPermissions.Contains(x.Permission))
            .ToListAsync();

        return HandleMenuTree(menus);
    }

    public async Task<List<MenuResponse>> GetMenusAsync(bool withButton)
    {
        var menus = await _freeSql
            .Select<LemonMenu>()
            .WhereIf(!withButton, x => x.MenuType == MenuConstants.Types.Menu)
            .OrderBy(x => x.Sort)
            .OrderBy(x => x.Id)
            .ToListAsync();

        return HandleMenuTree(menus);
    }

    public async Task CreateMenuAsync(MenuCreateRequest request)
    {
        var isPermissionExist = await _freeSql
            .Select<LemonMenu>()
            .Where(x => x.Permission == request.Permission)
            .AnyAsync();

        if (isPermissionExist)
        {
            throw new BusinessException("已存在相同的权限");
        }

        if (request.MenuType == MenuConstants.Types.Menu)
        {
            var isPathExist = await _freeSql
                .Select<LemonMenu>()
                .Where(x => x.ParentId == request.ParentId && x.Path == request.Path)
                .AnyAsync();

            if (isPathExist)
            {
                throw new BusinessException("已存在相同的菜单路径");
            }
        }

        var menu = request.Adapt<LemonMenu>();

        await _freeSql.Insert(menu).ExecuteAffrowsAsync();

        await _permissionService.RefreshAsync();
    }

    public async Task UpdateMenuAsync(int id, MenuUpdateRequest request)
    {
        var isPermissionExist = await _freeSql
            .Select<LemonMenu>()
            .Where(x => x.Permission == request.Permission && x.Id != id)
            .AnyAsync();

        if (isPermissionExist)
        {
            throw new BusinessException("已存在相同的权限");
        }

        var menu = await _freeSql.Select<LemonMenu>().Where(x => x.Id == id).ToOneAsync();

        if (menu == null || menu.IsSystem)
        {
            throw new BusinessException();
        }

        if (menu.MenuType == MenuConstants.Types.Menu)
        {
            if (string.IsNullOrEmpty(request.Path))
            {
                throw new BusinessException("菜单路径不能为空");
            }

            var isPathExist = await _freeSql
                .Select<LemonMenu>()
                .Where(x => x.ParentId == menu.ParentId && x.Path == request.Path && x.Id != id)
                .AnyAsync();

            if (isPathExist)
            {
                throw new BusinessException("已存在相同的菜单路径");
            }
        }

        await _freeSql
            .Update<LemonMenu>()
            .SetDto(request)
            .Where(x => x.Id == id)
            .ExecuteAffrowsAsync();

        await _permissionService.RefreshAsync();
    }

    private async Task<List<int>> GetAllChildrenIdsAsync(int id)
    {
        var allChildrenIds = new List<int>();

        var directChildren = await _freeSql
            .Select<LemonMenu>()
            .Where(x => x.ParentId == id)
            .ToListAsync(x => x.Id);

        allChildrenIds.AddRange(directChildren);

        foreach (var childId in directChildren)
        {
            var grandChildren = await GetAllChildrenIdsAsync(childId);
            allChildrenIds.AddRange(grandChildren);
        }

        return allChildrenIds;
    }

    public async Task DeleteMenuAsync(int id)
    {
        var menu = await _freeSql.Select<LemonMenu>().Where(x => x.Id == id).ToOneAsync();

        if (menu == null || menu.IsSystem)
        {
            throw new BusinessException();
        }

        var childrenIds = await GetAllChildrenIdsAsync(id);

        var count = await _freeSql.Delete<LemonMenu>().Where(x => x.Id == id).ExecuteAffrowsAsync();

        if (count == 0)
        {
            throw new BusinessException();
        }

        if (childrenIds.Count > 0)
        {
            await _freeSql
                .Delete<LemonMenu>()
                .Where(x => childrenIds.Contains(x.Id))
                .ExecuteAffrowsAsync();
        }

        await _permissionService.RefreshAsync();
    }
}
