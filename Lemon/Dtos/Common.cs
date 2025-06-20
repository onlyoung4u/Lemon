using System.ComponentModel.DataAnnotations;

namespace Lemon.Dtos;

public class PageQueryRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "页码必须大于0")]
    public int Page { get; set; } = 1;

    [Range(1, 200, ErrorMessage = "每页大小必须在1-200之间")]
    public int Limit { get; set; } = 10;
}

public class PageResponse<T>
{
    public List<T> Items { get; set; } = [];

    public long Total { get; set; }
}
