namespace Lemon.Dtos;

public class UserQueryRequest : PageQueryRequest
{
    public int? UserId { get; set; }
    public string? Username { get; set; }
    public string? Nickname { get; set; }
}

public class UserRole
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class UserResponse
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Nickname { get; set; }
    public bool IsActive { get; set; }
    public UserRole[] Roles { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UserCreateRequest
{
    public string? Username { get; set; }
    public string? Nickname { get; set; }
    public string? Password { get; set; }
    public bool? IsActive { get; set; }
    public int[]? Roles { get; set; }
}

public class UserUpdateRequest
{
    public string? Nickname { get; set; }
    public string? Password { get; set; }
    public bool? IsActive { get; set; }
    public int[]? Roles { get; set; }
}

public class UserActiveRequest
{
    public bool? IsActive { get; set; }
}
