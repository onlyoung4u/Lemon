using Lemon.Dtos;

namespace Lemon.Business.System;

public interface IRoleService
{
    Task<List<RoleResponse>> GetUserRolesAsync(int userId);
    Task<PageResponse<RoleResponse>> GetRolesAsync(RoleQueryRequest request, int userId);
    Task CreateRoleAsync(RoleCreateRequest request, int userId);
    Task UpdateRoleAsync(int id, RoleUpdateRequest request, int userId);
    Task DeleteRoleAsync(int id, int userId);
}
