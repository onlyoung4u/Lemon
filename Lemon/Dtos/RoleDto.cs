namespace Lemon.Dtos;

public class RoleQueryRequest : PageQueryRequest
{
    public string? Name { get; set; }
}

public class RoleCreator
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Nickname { get; set; }
}

public class RoleResponse
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string[]? Permissions { get; set; }
    public RoleCreator? Creator { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class RoleCreateRequest
{
    public string? Name { get; set; }
    public string[]? Permissions { get; set; }
}

public class RoleUpdateRequest
{
    public string? Name { get; set; }
    public string[]? Permissions { get; set; }
}
