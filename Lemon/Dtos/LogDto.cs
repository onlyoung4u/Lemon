using System.ComponentModel.DataAnnotations;

namespace Lemon.Dtos;

public class LogQueryRequest
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

    /// <summary>
    /// 页码
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "页码必须大于0")]
    public int Page { get; set; } = 1;

    /// <summary>
    /// 每页大小
    /// </summary>
    [Range(1, 100, ErrorMessage = "每页大小必须在1-100之间")]
    public int Limit { get; set; } = 10;
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
