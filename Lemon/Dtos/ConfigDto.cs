namespace Lemon.Dtos;

public class ConfigQueryRequest : PageQueryRequest
{
    public string? Name { get; set; }
    public string? Key { get; set; }
}

public class ConfigResponse
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Key { get; set; }
    public string Remark { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ConfigDetailResponse : ConfigResponse
{
    public string Value { get; set; }
}

public class ConfigRequest
{
    public string? Name { get; set; }
    public string? Key { get; set; }
    public string? Value { get; set; }
    public string? Remark { get; set; }
    public bool IsActive { get; set; }
}
