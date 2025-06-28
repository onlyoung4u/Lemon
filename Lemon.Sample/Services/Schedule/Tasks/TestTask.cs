using Coravel.Invocable;

namespace Lemon.Sample.Services.Schedule.Tasks;

public class TestTask(ILogger<TestTask> logger) : IInvocable
{
    private readonly ILogger<TestTask> _logger = logger;

    public async Task Invoke()
    {
        _logger.LogInformation("TestTask Start");
        await Task.Delay(2000);
        _logger.LogInformation("TestTask Done");
    }
}
