using Lemon.Services.Database;
using Lemon.Services.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lemon.Services.Extensions;

public static class ApplicationBuilderExtensions
{
    public static WebApplication UseLemon(this WebApplication app, bool isDevelopment = false)
    {
        // 数据库填充
        if (isDevelopment && app.Configuration.GetValue("Switch:DataSeed", false))
        {
            app.UseLemonDataSeed();
        }

        // 配置转发头处理，用于正确获取客户端IP
        app.UseForwardedHeaders(
            new ForwardedHeadersOptions
            {
                ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
            }
        );

        // 跨域
        app.UseCors("LemonCorsPolicy");

        // 异常处理
        app.UseLemonExceptionHandler();

        // JWT认证
        app.UseJwtAuth();

        // 权限检查和日志记录
        if (app.Configuration.GetValue("Switch:Admin", true))
        {
            app.UsePermissionAndLog();
        }

        return app;
    }

    /// <summary>
    /// 使用全局异常处理中间件
    /// </summary>
    /// <param name="app">应用程序构建器</param>
    /// <returns>应用程序构建器</returns>
    public static IApplicationBuilder UseLemonExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlerMiddleware>();
    }

    /// <summary>
    /// 使用JWT认证中间件
    /// </summary>
    public static IApplicationBuilder UseJwtAuth(this IApplicationBuilder app)
    {
        return app.UseMiddleware<JwtAuthMiddleware>();
    }

    /// <summary>
    /// 使用权限检查和日志记录中间件
    /// </summary>
    public static IApplicationBuilder UsePermissionAndLog(this IApplicationBuilder app)
    {
        return app.UseMiddleware<PermissionAndLogMiddleware>();
    }

    /// <summary>
    /// 使用数据库填充（在应用启动时执行）
    /// </summary>
    /// <param name="app">Web应用程序</param>
    /// <param name="enabled">是否在应用启动时执行填充</param>
    /// <returns>Web应用程序</returns>
    public static WebApplication UseLemonDataSeed(this WebApplication app)
    {
        _ = Task.Run(async () =>
        {
            using var scope = app.Services.CreateScope();
            var seedManager = scope.ServiceProvider.GetRequiredService<DataSeedManager>();
            var logger = scope.ServiceProvider.GetService<ILogger<DataSeedManager>>();

            try
            {
                await seedManager.SeedAllAsync();
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "应用启动时数据库填充失败");
            }
        });

        return app;
    }

    /// <summary>
    /// 手动执行数据库填充
    /// </summary>
    /// <param name="app">Web应用程序</param>
    /// <returns>填充任务</returns>
    public static async Task SeedDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var seedManager = scope.ServiceProvider.GetRequiredService<DataSeedManager>();
        await seedManager.SeedAllAsync();
    }
}
