using System.ComponentModel.DataAnnotations;

namespace Lemon.Dtos;

public class LogQueryRequest : PageQueryRequest
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public int? UserId { get; set; }

    /// <summary>
    /// 创建时间开始日期
    /// </summary>
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// 创建时间结束日期
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// 请求方法
    /// </summary>
    public string? Method { get; set; }
}

public class LogResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; }
    public string Nickname { get; set; }
    public string Path { get; set; }
    public string Description { get; set; }
    public string Method { get; set; }
    public string Ip { get; set; }
    public string Body { get; set; }
    public bool Success { get; set; }
    public DateTime CreatedAt { get; set; }
}
