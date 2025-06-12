namespace Lemon.Services.Attributes;

/// <summary>
/// 权限和日志属性 - 用于标记需要权限检查和日志记录的Action
/// </summary>
/// <param name="permission">权限代码</param>
/// <param name="description">操作描述</param>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public class LemonAdminAttribute(string? permission = null, string? description = null) : Attribute
{
    /// <summary>
    /// 权限代码
    /// </summary>
    public string? Permission { get; } = permission;

    /// <summary>
    /// 操作描述
    /// </summary>
    public string? Description { get; } = description;
}
