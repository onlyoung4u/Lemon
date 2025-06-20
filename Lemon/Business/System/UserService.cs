using Lemon.Business.Base;
using Lemon.Dtos;
using Lemon.Models;
using Lemon.Services.Exceptions;
using Lemon.Services.Permission;
using Lemon.Services.Utils;

namespace Lemon.Business.System;

public class UserService(IFreeSql freeSql, IPermissionService permissionService)
    : BaseService(freeSql),
        IUserService
{
    private readonly IPermissionService _permissionService = permissionService;

    public async Task<PageResponse<UserResponse>> GetUsersAsync(
        UserQueryRequest request,
        int userId
    )
    {
        var query = _freeSql
            .Select<LemonUser>()
            .IncludeMany(x => x.Roles)
            .Where(x => x.Id > 1)
            .Where(x => x.DeletedAt == null)
            .WhereIf(request.UserId.HasValue, x => x.Id == request.UserId.Value)
            .WhereIf(
                !string.IsNullOrEmpty(request.Username),
                x => x.Username.Contains(request.Username)
            )
            .WhereIf(
                !string.IsNullOrEmpty(request.Nickname),
                x => x.Nickname.Contains(request.Nickname)
            )
            .OrderBy(x => x.Id);

        var total = await query.CountAsync();

        var users = await query.Page(request.Page, request.Limit).ToListAsync();

        var items = users
            .Select(user => new UserResponse
            {
                Id = user.Id,
                Username = user.Username,
                Nickname = user.Nickname,
                IsActive = user.IsActive,
                Roles =
                    user.Roles?.Select(r => new UserRole { Id = r.Id, Name = r.Name }).ToArray()
                    ?? [],
                CreatedAt = user.CreatedAt,
            })
            .ToList();

        return new() { Items = items, Total = total };
    }

    public async Task CreateUserAsync(UserCreateRequest request, int userId)
    {
        // 检查用户名是否已存在
        var isUsernameExist = await _freeSql
            .Select<LemonUser>()
            .Where(x => x.Username == request.Username && x.DeletedAt == null)
            .AnyAsync();

        if (isUsernameExist)
        {
            throw new BusinessException("用户名已存在");
        }

        var roleIds = request.Roles.Distinct().ToArray();

        // 验证角色是否存在
        var existingRoleCount = await _freeSql
            .Select<LemonRole>()
            .WhereIf(userId > 1, x => x.CreatorId == userId)
            .Where(x => roleIds.Contains(x.Id))
            .CountAsync();

        if (existingRoleCount != roleIds.Length)
        {
            throw new BusinessException("存在无效的角色");
        }

        var isActive = request.IsActive ?? true;

        // 创建用户
        var user = new LemonUser
        {
            Username = request.Username,
            Nickname = request.Nickname,
            Password = BCryptHelper.HashPassword(request.Password),
            IsActive = isActive,
        };

        _freeSql.Transaction(() =>
        {
            // 插入用户
            var userId = (int)_freeSql.Insert(user).ExecuteIdentity();

            if (userId == 0)
            {
                throw new BusinessException();
            }

            // 插入用户角色关联
            var userRoles = roleIds
                .Select(roleId => new LemonUserRole { UserId = userId, RoleId = roleId })
                .ToList();

            var insertedCount = _freeSql.Insert(userRoles).ExecuteAffrows();

            if (insertedCount == 0)
            {
                throw new BusinessException();
            }

            if (!isActive)
            {
                // 处理非活跃用户记录
                var inactiveUserCount = _freeSql
                    .Select<LemonInactiveUser>()
                    .Where(x => x.UserId == userId)
                    .Where(x => x.Group == "admin")
                    .Count();

                if (inactiveUserCount == 0)
                {
                    _freeSql
                        .Insert(new LemonInactiveUser { UserId = userId, Group = "admin" })
                        .ExecuteAffrows();
                }
            }
        });
    }

    public async Task UpdateUserAsync(int id, UserUpdateRequest request, int userId)
    {
        if (id == 1)
        {
            throw new BusinessException();
        }

        // 检查用户是否存在
        var user =
            await _freeSql
                .Select<LemonUser>()
                .Where(x => x.Id == id && x.DeletedAt == null)
                .ToOneAsync() ?? throw new BusinessException("用户不存在");

        var isActive = request.IsActive ?? true;

        var roleIds = request.Roles.Distinct().ToArray();

        // 验证角色是否存在
        var existingRoleCount = await _freeSql
            .Select<LemonRole>()
            .WhereIf(userId > 1, x => x.CreatorId == userId)
            .Where(x => roleIds.Contains(x.Id))
            .CountAsync();

        if (existingRoleCount != roleIds.Length)
        {
            throw new BusinessException("存在无效的角色");
        }

        _freeSql.Transaction(() =>
        {
            // 更新用户信息
            var updateQuery = _freeSql
                .Update<LemonUser>()
                .Where(x => x.Id == id)
                .Set(x => x.Nickname, request.Nickname)
                .Set(x => x.IsActive, isActive);

            if (!string.IsNullOrEmpty(request.Password))
            {
                updateQuery.Set(x => x.Password, BCryptHelper.HashPassword(request.Password));
            }

            updateQuery.ExecuteAffrows();

            // 删除原有的角色关联
            var deletedCount = _freeSql
                .Delete<LemonUserRole>()
                .Where(x => x.UserId == id)
                .ExecuteAffrows();

            if (deletedCount == 0)
            {
                throw new BusinessException();
            }

            // 添加新的角色关联
            var userRoles = roleIds
                .Select(roleId => new LemonUserRole { UserId = id, RoleId = roleId })
                .ToList();

            var insertedCount = _freeSql.Insert(userRoles).ExecuteAffrows();

            if (insertedCount == 0)
            {
                throw new BusinessException();
            }

            // 处理非活跃用户记录
            if (isActive)
            {
                _freeSql.Delete<LemonInactiveUser>().Where(x => x.UserId == id).ExecuteAffrows();
            }
            else
            {
                var inactiveUserCount = _freeSql
                    .Select<LemonInactiveUser>()
                    .Where(x => x.UserId == id)
                    .Where(x => x.Group == "admin")
                    .Count();

                if (inactiveUserCount == 0)
                {
                    _freeSql
                        .Insert(new LemonInactiveUser { UserId = id, Group = "admin" })
                        .ExecuteAffrows();
                }
            }
        });

        await _permissionService.RefreshAsync();
    }

    public async Task DeleteUserAsync(int id, int userId)
    {
        if (id == 1 || id == userId)
        {
            throw new BusinessException();
        }

        // 检查用户是否存在
        var user =
            await _freeSql
                .Select<LemonUser>()
                .Where(x => x.Id == id && x.DeletedAt == null)
                .ToOneAsync() ?? throw new BusinessException("用户不存在");

        _freeSql.Transaction(() =>
        {
            // 软删除用户
            var affectedRows = _freeSql
                .Update<LemonUser>()
                .Set(x => x.DeletedAt, DateTime.Now)
                .Set(x => x.Username, $"{user.Username}_deleted_{DateTime.Now.Ticks}")
                .Where(x => x.Id == id)
                .ExecuteAffrows();

            if (affectedRows == 0)
            {
                throw new BusinessException("删除用户失败");
            }

            // 删除用户角色关联
            var deletedCount = _freeSql
                .Delete<LemonUserRole>()
                .Where(x => x.UserId == id)
                .ExecuteAffrows();

            if (deletedCount == 0)
            {
                throw new BusinessException();
            }

            // 处理非活跃用户记录
            var inactiveUserCount = _freeSql
                .Select<LemonInactiveUser>()
                .Where(x => x.UserId == id)
                .Where(x => x.Group == "admin")
                .Count();

            if (inactiveUserCount == 0)
            {
                _freeSql
                    .Insert(new LemonInactiveUser { UserId = id, Group = "admin" })
                    .ExecuteAffrows();
            }
        });

        await _permissionService.RefreshAsync();
    }

    public async Task SetUserActiveAsync(int id, bool isActive, int userId)
    {
        if (id == 1 || id == userId)
        {
            throw new BusinessException();
        }

        // 检查用户是否存在
        var user =
            await _freeSql
                .Select<LemonUser>()
                .Where(x => x.Id == id && x.DeletedAt == null)
                .ToOneAsync() ?? throw new BusinessException("用户不存在");

        _freeSql.Transaction(() =>
        {
            var updatedCount = _freeSql
                .Update<LemonUser>()
                .Set(x => x.IsActive, isActive)
                .Where(x => x.Id == id)
                .ExecuteAffrows();

            if (updatedCount == 0)
            {
                throw new BusinessException();
            }

            if (isActive)
            {
                _freeSql.Delete<LemonInactiveUser>().Where(x => x.UserId == id).ExecuteAffrows();
            }
            else
            {
                var inactiveUserCount = _freeSql
                    .Select<LemonInactiveUser>()
                    .Where(x => x.UserId == id)
                    .Where(x => x.Group == "admin")
                    .Count();

                if (inactiveUserCount == 0)
                {
                    _freeSql
                        .Insert(new LemonInactiveUser { UserId = id, Group = "admin" })
                        .ExecuteAffrows();
                }
            }
        });
    }
}
