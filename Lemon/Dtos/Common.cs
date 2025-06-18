namespace Lemon.Dtos;

public class PageResponse<T>
{
    public List<T> Items { get; set; } = [];

    public long Total { get; set; }
}
