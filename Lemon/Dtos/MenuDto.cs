namespace Lemon.Dtos;

public class MenuCreateRequest
{
    public int MenuType { get; set; }

    public int ParentId { get; set; }

    public string? Title { get; set; }

    public string? Permission { get; set; }

    public string Path { get; set; } = string.Empty;

    public string Icon { get; set; } = string.Empty;

    public string Link { get; set; } = string.Empty;

    public int Sort { get; set; }
}

public class MenuUpdateRequest
{
    public string? Title { get; set; }

    public string? Permission { get; set; }

    public string? Path { get; set; }

    public string? Icon { get; set; }

    public string? Link { get; set; }

    public int Sort { get; set; }
}

public class MenuItem
{
    public int Id { get; set; }

    public int MenuType { get; set; }

    public int ParentId { get; set; }

    public string Title { get; set; }

    public string Permission { get; set; }

    public string Path { get; set; }

    public string Icon { get; set; }

    public string Link { get; set; }

    public int Sort { get; set; }

    public bool IsSystem { get; set; }
}

public class MenuResponse : MenuItem
{
    public List<MenuResponse>? Children { get; set; }
}
