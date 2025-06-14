using FreeSql.DataAnnotations;

namespace Lemon.Models;

[Index("uk_group_user_id", "Group, UserId", true)]
public class LemonInactiveUser : LemonBaseWithoutTimestamps
{
    [Column(StringLength = 64)]
    public string Group { get; set; }

    public int UserId { get; set; }
}
