using Coravel;
using Lemon.Sample.Services.Schedule.Tasks;

namespace Lemon.Sample.Services.Schedule;

public static class ScheduleServiceExtensions
{
    /// <summary>
    /// 注册调度器服务和任务
    /// </summary>
    public static IServiceCollection AddSchedulers(this IServiceCollection services)
    {
        services.AddScheduler();

        // 注册所有定时任务
        services.AddTransient<TestTask>();

        return services;
    }

    /// <summary>
    /// 配置定时任务调度
    /// </summary>
    public static IApplicationBuilder UseSchedulers(this IApplicationBuilder app)
    {
        app.ApplicationServices.UseScheduler(scheduler =>
        {
            scheduler.Schedule<TestTask>().EveryMinute();
        });

        return app;
    }
}
