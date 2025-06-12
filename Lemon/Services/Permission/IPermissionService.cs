namespace Lemon.Services.Permission;

public interface IPermissionService
{
    Task<int> GetFlagAsync();

    Task<int> RefreshAsync();

    Task<bool> CheckPermissionAsync(int userId, string permission);

    Task<List<string>> GetUserPermissionsAsync(int userId);
}
