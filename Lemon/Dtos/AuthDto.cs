using System.ComponentModel.DataAnnotations;

namespace Lemon.Dtos;

/// <summary>
/// 登录请求模型
/// </summary>
public class LoginRequest
{
    public string? Username { get; set; }

    public string? Password { get; set; }
}

public class LoginResponse
{
    public required string Token { get; set; }
}

public class UserInfoResponse
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public required int UserId { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public required string Username { get; set; }

    /// <summary>
    /// 昵称
    /// </summary>
    public required string Nickname { get; set; }

    /// <summary>
    /// 角色
    /// </summary>
    public required List<string> Roles { get; set; }

    /// <summary>
    /// 权限
    /// </summary>
    public required List<string> Permissions { get; set; }
}

public class MenuMeta
{
    public required string Title { get; set; }
    public string? Permission { get; set; }
    public string? Icon { get; set; }
    public string? Href { get; set; }
}

public class MenuResponse
{
    public required string Name { get; set; }
    public required string Path { get; set; }
    public required string Component { get; set; }
    public required MenuMeta Meta { get; set; }
    public List<MenuResponse>? Children { get; set; }
}

public class ChangePasswordRequest
{
    public string? OldPassword { get; set; }
    public string? NewPassword { get; set; }
}
