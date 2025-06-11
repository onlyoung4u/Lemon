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
