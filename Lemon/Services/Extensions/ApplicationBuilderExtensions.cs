using Lemon.Services.Exceptions;
using Microsoft.AspNetCore.Builder;

namespace Lemon.Services.Extensions;

public static class ApplicationBuilderExtensions
{
    public static WebApplication UseLemon(this WebApplication app)
    {
        // 跨域
        app.UseCors("LemonCorsPolicy");

        // 异常处理
        app.UseLemonExceptionHandler();

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
}
