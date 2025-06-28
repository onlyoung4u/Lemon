using Lemon.Sample.Services.Queue.Handler;

namespace Lemon.Sample.Services.Queue;

public static class QueueServiceExtensions
{
    /// <summary>
    /// 注册队列消费者服务
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddQueueConsumers(this IServiceCollection services)
    {
        // 注册所有消费者
        services.AddScoped<TestEventHandler>();

        return services;
    }
}
