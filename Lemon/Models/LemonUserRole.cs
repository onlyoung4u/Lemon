using FreeSql.DataAnnotations;

namespace Lemon.Models;

[Index("uk_user_id_role_id", "UserId, RoleId", true)]
public class LemonUserRole : LemonBaseWithoutTimestamps
{
    public int UserId { get; set; }

    public int RoleId { get; set; }

    [Navigate(nameof(UserId))]
    public LemonUser User { get; set; }

    [Navigate(nameof(RoleId))]
    public LemonRole Role { get; set; }
}
