using FreeSql.DataAnnotations;

namespace Lemon.Models;

[Index("uk_name", "Name", true)]
public class LemonRole : LemonBase
{
    [Column(StringLength = 64)]
    public string Name { get; set; }

    public int CreatorId { get; set; }

    [Navigate(nameof(CreatorId))]
    public LemonUser Creator { get; set; }

    [Navigate(nameof(LemonRolePermission.RoleId))]
    public List<LemonRolePermission> Permissions { get; set; }

    [Navigate(ManyToMany = typeof(LemonUserRole))]
    public List<LemonUser> Users { get; set; }
}
