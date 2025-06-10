using FreeSql.DataAnnotations;

namespace Lemon.Models;

[Index("uk_username", "Username", true)]
public class LemonUser : LemonBaseWithSoftDelete
{
    [Column(StringLength = 64)]
    public string Username { get; set; }

    [Column(StringLength = 64)]
    public string Nickname { get; set; }

    public string Password { get; set; }

    [Column(StringLength = 39)]
    public string LastLoginIp { get; set; } = string.Empty;

    [Column(IsNullable = true)]
    public DateTime? LastLoginTime { get; set; }

    public bool IsActive { get; set; } = true;

    [Navigate(ManyToMany = typeof(LemonUserRole))]
    public List<LemonRole> Roles { get; set; }
}
