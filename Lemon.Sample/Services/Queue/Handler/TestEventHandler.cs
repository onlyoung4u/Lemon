using DotNetCore.CAP;
using Lemon.Sample.Services.Queue.Model;

namespace Lemon.Sample.Services.Queue.Handler;

public class TestEventHandler(ILogger<TestEventHandler> logger) : ICapSubscribe
{
    private readonly ILogger<TestEventHandler> _logger = logger;

    [CapSubscribe("test")]
    public async Task Handle(TestQueue testQueue)
    {
        // 记录日志
        _logger.LogInformation("Received Id: {Id}", testQueue.Id);
        _logger.LogInformation("Received Name: {Name}", testQueue.Name);

        // 模拟处理
        await Task.Delay(1000);

        // 记录处理完成
        _logger.LogInformation("处理完成");
    }
}
