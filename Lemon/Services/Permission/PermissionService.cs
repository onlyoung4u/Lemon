using Lemon.Models;
using Lemon.Services.Cache;

namespace Lemon.Services.Permission;

public class PermissionService(IFreeSql freeSql, IHybridCacheService cacheService)
    : IPermissionService
{
    private readonly IFreeSql _freeSql = freeSql;
    private readonly IHybridCacheService _cacheService = cacheService;

    public async Task<int> GetFlagAsync()
    {
        var flag = await _cacheService.GetFromMemoryCacheAsync<int>("permission:flag");
        if (flag == 0)
        {
            flag = await RefreshAsync();
        }
        return flag;
    }

    public async Task<int> RefreshAsync()
    {
        var flag = (int)DateTimeOffset.Now.ToUnixTimeSeconds();
        await _cacheService.SetMemoryCacheAsync("permission:flag", flag);
        return flag;
    }

    public async Task<bool> CheckPermissionAsync(int userId, string permission)
    {
        if (userId == 1)
        {
            return true;
        }

        var permissions = await GetUserPermissionsAsync(userId);

        return permissions.Contains(permission);
    }

    public async Task<List<string>> GetUserPermissionsAsync(int userId)
    {
        var flag = await GetFlagAsync();
        var cacheKey = $"permission:user:{userId}:{flag}";
        var cachePermissions = await _cacheService.GetFromMemoryCacheAsync<List<string>>(cacheKey);

        if (cachePermissions != null)
        {
            return cachePermissions;
        }

        var permissions = new List<string>();

        if (userId == 1)
        {
            permissions = await _freeSql.Select<LemonMenu>().ToListAsync(x => x.Permission);
        }
        else
        {
            var roleIds = await _freeSql
                .Select<LemonUserRole>()
                .Where(x => x.UserId == userId)
                .ToListAsync(x => x.RoleId);

            if (roleIds.Count > 0)
            {
                permissions = await _freeSql
                    .Select<LemonRolePermission>()
                    .Where(x => roleIds.Contains(x.RoleId))
                    .Distinct()
                    .ToListAsync(x => x.Permission);
            }
        }

        await _cacheService.SetMemoryCacheAsync(cacheKey, permissions);

        return permissions;
    }
}
