using FreeSql.DataAnnotations;

namespace Lemon.Models;

[Index("uk_permission", "Permission", true)]
public class LemonMenu : LemonBase
{
    public int MenuType { get; set; }

    public int ParentId { get; set; }

    [Column(StringLength = 64)]
    public string Title { get; set; }

    public string Path { get; set; }

    public string Permission { get; set; }

    [Column(StringLength = 64)]
    public string Icon { get; set; } = string.Empty;

    public string Link { get; set; } = string.Empty;

    public int Sort { get; set; }

    public bool IsSystem { get; set; } = false;
}
