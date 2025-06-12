using FreeSql.DataAnnotations;

namespace Lemon.Models;

[Index("idx_user_id", "UserId")]
public class LemonOperationLog : LemonBase
{
    public int UserId { get; set; }

    [Column(StringLength = 64)]
    public string Username { get; set; }

    [Column(StringLength = 64)]
    public string Nickname { get; set; }

    public string Path { get; set; }

    public string Description { get; set; }

    [Column(StringLength = 10)]
    public string Method { get; set; }

    [Column(StringLength = 39)]
    public string Ip { get; set; }

    [Column(StringLength = -1)]
    public string Body { get; set; }

    public bool Success { get; set; }
}
