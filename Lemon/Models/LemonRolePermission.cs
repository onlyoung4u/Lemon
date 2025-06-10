using FreeSql.DataAnnotations;

namespace Lemon.Models;

[Index("uk_role_id_permission", "RoleId, Permission", true)]
public class LemonRolePermission : LemonBaseWithoutTimestamps
{
    public int RoleId { get; set; }

    [Column(StringLength = 64)]
    public string Permission { get; set; }
}
