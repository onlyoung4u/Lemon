using Lemon.Dtos;

namespace Lemon.Business.Auth;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, string clientIp);
    Task LogoutAsync(string token);
    Task<UserInfoResponse> GetUserInfoAsync(int userId);
    Task<List<MenuResponse>> GetMenusAsync(int userId);
}
