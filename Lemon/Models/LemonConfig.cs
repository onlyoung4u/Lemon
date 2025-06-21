using FreeSql.DataAnnotations;

namespace Lemon.Models;

[Index("uk_key", "Key", true)]
public class LemonConfig : LemonBase
{
    [Column(StringLength = 64)]
    public string Name { get; set; }

    [Column(StringLength = 64)]
    public string Key { get; set; }

    [Column(StringLength = -1)]
    public string Value { get; set; }

    [Column(StringLength = 255)]
    public string Remark { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
