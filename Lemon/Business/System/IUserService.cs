using Lemon.Dtos;

namespace Lemon.Business.System;

public interface IUserService
{
    Task<PageResponse<UserResponse>> GetUsersAsync(UserQueryRequest request, int userId);
    Task CreateUserAsync(UserCreateRequest request, int userId);
    Task UpdateUserAsync(int id, UserUpdateRequest request, int userId);
    Task DeleteUserAsync(int id, int userId);
    Task SetUserActiveAsync(int id, bool isActive, int userId);
}
