using System.ComponentModel.DataAnnotations;

namespace Lemon.Dtos;

/// <summary>
/// 登录请求模型
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// 用户名
    /// </summary>
    [Required(ErrorMessage = "用户名不能为空")]
    [StringLength(64, MinimumLength = 4, ErrorMessage = "用户名长度必须在4到64个字符之间")]
    public string Username { get; set; }

    /// <summary>
    /// 密码
    /// </summary>
    [Required(ErrorMessage = "密码不能为空")]
    [StringLength(20, MinimumLength = 6, ErrorMessage = "密码长度必须在6到20个字符之间")]
    public string Password { get; set; }
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
