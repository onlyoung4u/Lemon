using DotNetCore.CAP;
using Lemon.Controllers;
using Lemon.Sample.Services.Queue.Model;
using Lemon.Services.Response;
using Microsoft.AspNetCore.Mvc;

namespace Lemon.Sample.Controllers;

[ApiController]
[Route("api/queue")]
public class QueueController(IResponseBuilder responseBuilder, ICapPublisher capPublisher)
    : LemonController(responseBuilder)
{
    private readonly ICapPublisher _capPublisher = capPublisher;

    [HttpPost("test")]
    public async Task<IActionResult> Test()
    {
        // 发布消息
        await _capPublisher.PublishAsync("test", new TestQueue { Id = 1, Name = "test" });

        // 发布延迟消息
        await _capPublisher.PublishDelayAsync(
            TimeSpan.FromSeconds(10),
            "test",
            new TestQueue { Id = 2, Name = "delay" }
        );

        return Success();
    }
}
