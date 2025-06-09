using System.Text.Json.Serialization;
using FluentValidation;
using FreeSql;
using Lemon.Services.Cache;
using Lemon.Services.Jwt;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Lemon.Services.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加基础服务配置
    /// </summary>
    public static IServiceCollection AddLemonServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        // 配置Json序列化选项
        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.PropertyNameCaseInsensitive = true;
            options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        });

        // 添加跨域
        services.AddLemonCors(configuration);

        // 添加数据库服务
        services.AddFreeSql(configuration);

        // 添加混合缓存服务
        services.AddHybridCache(configuration);

        // 添加JWT服务
        services.AddJwtService(configuration);

        // 添加业务服务
        // services.AddBusinessServices();

        // 添加验证器
        // services.AddFluentValidation();

        // 添加HTTP上下文访问器
        // services.AddHttpContextAccessor();

        return services;
    }

    private static IServiceCollection AddLemonCors(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var corsOptions = configuration.GetSection("Cors");
        var origins = corsOptions.GetSection("AllowedOrigins").Get<string[]>() ?? [];

        services.AddCors(options =>
        {
            options.AddPolicy(
                "LemonCorsPolicy",
                policy =>
                {
                    if (origins.Length > 0)
                    {
                        policy
                            .WithOrigins(origins)
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials();
                    }
                    else
                    {
                        policy
                            .AllowAnyOrigin()
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials();
                    }
                }
            );
        });

        return services;
    }

    /// <summary>
    /// 添加FreeSql数据库服务
    /// </summary>
    private static IServiceCollection AddFreeSql(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var connectionString = configuration.GetConnectionString("Database");
        var dbType = configuration.GetValue("Database:Type", "MySQL");

        DataType dataType = dbType.ToUpper() switch
        {
            "MYSQL" => DataType.MySql,
            "POSTGRESQL" => DataType.PostgreSQL,
            "DAMENG" => DataType.Dameng,
            "SHENTONG" => DataType.ShenTong,
            "KINGBASE" => DataType.KingbaseES,
            "GBASE" => DataType.GBase,
            "XUGU" => DataType.Xugu,
            "HANGAO" => DataType.Custom,
            _ => throw new NotSupportedException($"不支持的数据库类型: {dbType}"),
        };

        var isDevelopment =
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

        var builder = new FreeSqlBuilder()
            .UseConnectionString(dataType, connectionString)
            .UseAdoConnectionPool(configuration.GetValue("Database:ConnectionPool", false))
            .UseAutoSyncStructure(
                isDevelopment && configuration.GetValue("Database:AutoSyncStructure", false)
            );

        if (isDevelopment)
        {
            builder.UseMonitorCommand(cmd => Console.WriteLine($"SQL: {cmd.CommandText}"));
        }

        var freeSql = builder.Build();

        services.AddSingleton(freeSql);

        return services;
    }

    /// <summary>
    /// 添加混合缓存服务
    /// </summary>
    private static IServiceCollection AddHybridCache(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        // 添加内存缓存
        services.AddMemoryCache();

        // 添加Redis缓存服务
        var connectionString = configuration.GetConnectionString("Redis");

        if (!string.IsNullOrEmpty(connectionString))
        {
            services.AddSingleton<IConnectionMultiplexer>(provider =>
            {
                return ConnectionMultiplexer.Connect(connectionString);
            });
        }

        // 注册混合缓存服务
        services.AddSingleton<IHybridCacheService, HybridCacheService>();

        return services;
    }

    /// <summary>
    /// 添加业务服务
    /// </summary>
    private static IServiceCollection AddBusinessServices(this IServiceCollection services)
    {
        // 注册业务服务

        return services;
    }

    /// <summary>
    /// 添加JWT服务
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    private static IServiceCollection AddJwtService(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var jwtSection = configuration.GetSection("Jwt");
        var jwtOptions = jwtSection.Get<List<JwtOptions>>();

        if (jwtOptions != null && jwtOptions.Count > 0)
        {
            services.Configure<List<JwtOptions>>(opt =>
            {
                opt.Clear();
                opt.AddRange(jwtOptions);
            });

            services.AddSingleton<IJwtService, JwtService>();
        }

        return services;
    }

    /// <summary>
    /// 添加FluentValidation
    /// </summary>
    private static IServiceCollection AddFluentValidation(this IServiceCollection services)
    {
        return services;
    }
}
