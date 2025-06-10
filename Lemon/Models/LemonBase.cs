using FreeSql.DataAnnotations;

namespace Lemon.Models;

public class LemonBase
{
    [Column(IsPrimary = true, IsIdentity = true)]
    public int Id { get; set; }

    [Column(ServerTime = DateTimeKind.Utc, CanUpdate = false)]
    public DateTime CreatedAt { get; set; }

    [Column(ServerTime = DateTimeKind.Utc)]
    public DateTime? UpdatedAt { get; set; }
}

public class LemonBaseWithoutTimestamps
{
    [Column(IsPrimary = true, IsIdentity = true)]
    public int Id { get; set; }
}

public class LemonBaseWithSoftDelete : LemonBase
{
    [Column(IsNullable = true)]
    public DateTime? DeletedAt { get; set; }
}
