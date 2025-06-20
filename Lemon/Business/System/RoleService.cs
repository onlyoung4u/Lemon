using Lemon.Business.Base;
using Lemon.Dtos;
using Lemon.Models;
using Lemon.Services.Exceptions;
using Lemon.Services.Permission;

namespace Lemon.Business.System;

public class RoleService(IFreeSql freeSql, IPermissionService permissionService)
    : BaseService(freeSql),
        IRoleService
{
    private readonly IPermissionService _permissionService = permissionService;

    public async Task<List<RoleResponse>> GetUserRolesAsync(int userId)
    {
        var roles = await _freeSql
            .Select<LemonRole>()
            .WhereIf(userId > 1, x => x.CreatorId == userId)
            .ToListAsync();

        return [.. roles.Select(role => new RoleResponse { Id = role.Id, Name = role.Name })];
    }

    public async Task<PageResponse<RoleResponse>> GetRolesAsync(
        RoleQueryRequest request,
        int userId
    )
    {
        var query = _freeSql
            .Select<LemonRole>()
            .IncludeMany(x => x.Permissions)
            .Include(x => x.Creator)
            .WhereIf(userId > 1, x => x.CreatorId == userId)
            .WhereIf(!string.IsNullOrEmpty(request.Name), x => x.Name.Contains(request.Name))
            .OrderBy(x => x.Id);

        var total = await query.CountAsync();

        var roles = await query.Page(request.Page, request.Limit).ToListAsync();

        var items = roles
            .Select(role => new RoleResponse
            {
                Id = role.Id,
                Name = role.Name,
                Permissions = role.Permissions?.Select(p => p.Permission).ToArray() ?? [],
                Creator = new RoleCreator
                {
                    Id = role.CreatorId,
                    Username = role.Creator.Username,
                    Nickname = role.Creator.Nickname,
                },
                CreatedAt = role.CreatedAt,
            })
            .ToList();

        return new() { Items = items, Total = total };
    }

    private async Task CheckPermissionsAsync(string[] permissions, int userId)
    {
        if (permissions.Length == 0)
        {
            throw new BusinessException("权限不能为空");
        }

        var existingPermissionCount = await _freeSql
            .Select<LemonMenu>()
            .Where(x => permissions.Contains(x.Permission))
            .CountAsync();

        if (existingPermissionCount != permissions.Length)
        {
            throw new BusinessException("存在未知的权限");
        }

        if (userId > 1)
        {
            var userPermissions = await _permissionService.GetUserPermissionsAsync(userId);

            foreach (var permission in permissions)
            {
                if (!userPermissions.Contains(permission))
                {
                    throw new BusinessException("越级赋权");
                }
            }
        }
    }

    public async Task CreateRoleAsync(RoleCreateRequest request, int userId)
    {
        var isNameExist = await _freeSql
            .Select<LemonRole>()
            .Where(x => x.Name == request.Name)
            .AnyAsync();

        if (isNameExist)
        {
            throw new BusinessException("角色名称已存在");
        }

        await CheckPermissionsAsync(request.Permissions, userId);

        // 创建角色
        var role = new LemonRole { Name = request.Name, CreatorId = userId };

        _freeSql.Transaction(() =>
        {
            // 插入角色并获取返回的ID
            var roleId = (int)_freeSql.Insert(role).ExecuteIdentity();

            if (roleId == 0)
            {
                throw new BusinessException();
            }

            // 如果有权限，则插入角色权限关联
            var rolePermissions = request
                .Permissions.Select(permission => new LemonRolePermission
                {
                    RoleId = roleId,
                    Permission = permission,
                })
                .ToList();

            var insertedCount = _freeSql.Insert(rolePermissions).ExecuteAffrows();

            if (insertedCount == 0)
            {
                throw new BusinessException();
            }
        });
    }

    public async Task UpdateRoleAsync(int id, RoleUpdateRequest request, int userId)
    {
        // 检查角色是否存在
        var role =
            await _freeSql.Select<LemonRole>().Where(x => x.Id == id).ToOneAsync()
            ?? throw new BusinessException("角色不存在");

        if (userId > 1 && role.CreatorId != userId)
        {
            throw new BusinessException("无权限修改其他用户创建的角色");
        }

        // 检查角色名称是否已被其他角色使用
        var isNameExist = await _freeSql
            .Select<LemonRole>()
            .Where(x => x.Name == request.Name && x.Id != id)
            .AnyAsync();

        if (isNameExist)
        {
            throw new BusinessException("角色名称已存在");
        }

        await CheckPermissionsAsync(request.Permissions, userId);

        _freeSql.Transaction(() =>
        {
            // 更新角色基本信息
            _freeSql
                .Update<LemonRole>()
                .Set(x => x.Name, request.Name)
                .Where(x => x.Id == id)
                .ExecuteAffrows();

            // 删除原有的权限关联
            var deletedCount = _freeSql
                .Delete<LemonRolePermission>()
                .Where(x => x.RoleId == id)
                .ExecuteAffrows();

            if (deletedCount == 0)
            {
                throw new BusinessException();
            }

            // 添加新的权限关联
            var rolePermissions = request
                .Permissions.Select(permission => new LemonRolePermission
                {
                    RoleId = id,
                    Permission = permission,
                })
                .ToList();

            var insertedCount = _freeSql.Insert(rolePermissions).ExecuteAffrows();

            if (insertedCount == 0)
            {
                throw new BusinessException();
            }
        });

        await _permissionService.RefreshAsync();
    }

    public async Task DeleteRoleAsync(int id, int userId)
    {
        // 检查角色是否存在
        var role =
            await _freeSql.Select<LemonRole>().Where(x => x.Id == id).ToOneAsync()
            ?? throw new BusinessException("角色不存在");

        if (userId > 1 && role.CreatorId != userId)
        {
            throw new BusinessException("无权限删除其他用户创建的角色");
        }

        // 检查角色是否被用户使用
        var isRoleInUse = await _freeSql
            .Select<LemonUserRole>()
            .Where(x => x.RoleId == id)
            .AnyAsync();

        if (isRoleInUse)
        {
            throw new BusinessException("角色正在被用户使用，无法删除");
        }

        _freeSql.Transaction(() =>
        {
            // 删除角色权限关联
            var deletedCount = _freeSql
                .Delete<LemonRolePermission>()
                .Where(x => x.RoleId == id)
                .ExecuteAffrows();

            if (deletedCount == 0)
            {
                throw new BusinessException();
            }

            // 删除角色
            deletedCount = _freeSql.Delete<LemonRole>().Where(x => x.Id == id).ExecuteAffrows();

            if (deletedCount == 0)
            {
                throw new BusinessException();
            }
        });
    }
}
