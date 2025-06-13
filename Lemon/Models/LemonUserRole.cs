using FreeSql.DataAnnotations;

namespace Lemon.Models;

public class LemonUserRole
{
    [Column(IsPrimary = true)]
    public int UserId { get; set; }

    [Column(IsPrimary = true)]
    public int RoleId { get; set; }

    [Navigate(nameof(UserId))]
    public LemonUser User { get; set; }

    [Navigate(nameof(RoleId))]
    public LemonRole Role { get; set; }
}
