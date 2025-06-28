namespace Lemon.Sample.Services.Queue;

public class QueueOptions
{
    public string Database { get; set; } = string.Empty;
    public string Redis { get; set; } = string.Empty;
    public string DefaultGroupName { get; set; } = "lemon.queue";
    public int FailedRetryCount { get; set; } = 3;
}
