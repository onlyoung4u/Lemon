using System.Reflection;
using System.Text.Json.Serialization;
using FluentValidation;
using FreeSql;
using Lemon.Services.Cache;
using Lemon.Services.Database;
using Lemon.Services.Jwt;
using Lemon.Services.Response;
using Microsoft.AspNetCore.Mvc;
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
        // 配置控制器
        services.ConfigureControllers();

        // 添加跨域
        services.AddLemonCors(configuration);

        // 添加数据库服务
        services.AddFreeSql(configuration);

        // 添加混合缓存服务
        services.AddHybridCache(configuration);

        // 添加JWT服务
        services.AddJwtService(configuration);

        // 添加响应服务
        services.AddResponseServices(configuration);

        // 添加验证器
        services.AddFluentValidation();

        // 添加数据库填充服务
        services.AddDataSeedServices();

        // 添加业务服务
        // services.AddBusinessServices();

        // 添加HTTP上下文访问器
        // services.AddHttpContextAccessor();

        return services;
    }

    /// <summary>
    /// 配置控制器
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <returns>服务集合</returns>
    private static IServiceCollection ConfigureControllers(this IServiceCollection services)
    {
        services
            .AddControllers()
            .ConfigureApplicationPartManager(manager =>
            {
                var lemonAssembly = Assembly.GetAssembly(typeof(ServiceCollectionExtensions));

                if (lemonAssembly != null)
                {
                    manager.ApplicationParts.Add(
                        new Microsoft.AspNetCore.Mvc.ApplicationParts.AssemblyPart(lemonAssembly)
                    );
                }
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                options.JsonSerializerOptions.DefaultIgnoreCondition =
                    JsonIgnoreCondition.WhenWritingNull;
            })
            .ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var responseBuilder =
                        context.HttpContext.RequestServices.GetRequiredService<IResponseBuilder>();

                    var firstError = context
                        .ModelState.Where(x => x.Value?.Errors?.Count > 0)
                        .SelectMany(x => x.Value!.Errors)
                        .Select(x => x.ErrorMessage)
                        .FirstOrDefault();

                    var response = responseBuilder.BadRequest(firstError ?? "参数错误");

                    return new OkObjectResult(response);
                };
            });

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
                        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
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
    /// 添加FluentValidation验证服务
    /// </summary>
    private static IServiceCollection AddFluentValidation(this IServiceCollection services)
    {
        // 自动注册当前程序集中的所有验证器
        services.AddValidatorsFromAssembly(typeof(ServiceCollectionExtensions).Assembly);

        return services;
    }

    /// <summary>
    /// 添加响应服务
    /// </summary>
    private static IServiceCollection AddResponseServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        // 注册响应消息服务
        services.AddSingleton<IResponseMessageService>(provider =>
        {
            var messageService = new ResponseMessageService();

            // 从配置文件中加载自定义状态码和消息
            LoadCustomMessagesFromConfiguration(messageService, configuration);

            return messageService;
        });

        // 注册响应构建器
        services.AddScoped<IResponseBuilder, ResponseBuilder>();

        return services;
    }

    /// <summary>
    /// 添加响应服务并配置自定义状态码和消息
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configuration">配置对象</param>
    /// <param name="configureOptions">额外的配置选项</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddResponseServices(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<ResponseOptions>? configureOptions = null
    )
    {
        // 注册响应消息服务
        services.AddSingleton<IResponseMessageService>(provider =>
        {
            var messageService = new ResponseMessageService();

            // 从配置文件中加载自定义状态码和消息
            LoadCustomMessagesFromConfiguration(messageService, configuration);

            // 如果有额外的自定义配置，应用自定义配置
            if (configureOptions != null)
            {
                var options = new ResponseOptions();
                configureOptions(options);
                messageService.SetMessages(options.CustomMessages);
            }

            return messageService;
        });

        // 注册响应构建器
        services.AddScoped<IResponseBuilder, ResponseBuilder>();

        return services;
    }

    /// <summary>
    /// 从配置文件中加载自定义状态码和消息
    /// </summary>
    /// <param name="messageService">消息服务</param>
    /// <param name="configuration">配置对象</param>
    private static void LoadCustomMessagesFromConfiguration(
        ResponseMessageService messageService,
        IConfiguration configuration
    )
    {
        try
        {
            var responseSection = configuration.GetSection(ResponseConfiguration.SectionName);
            if (!responseSection.Exists())
            {
                return;
            }

            var customMessagesSection = responseSection.GetSection("CustomMessages");
            if (!customMessagesSection.Exists())
            {
                return;
            }

            var customMessages = new Dictionary<int, string>();

            foreach (var item in customMessagesSection.GetChildren())
            {
                if (int.TryParse(item.Key, out var code) && !string.IsNullOrEmpty(item.Value))
                {
                    customMessages[code] = item.Value;
                }
            }

            if (customMessages.Count > 0)
            {
                messageService.SetMessages(customMessages);
            }
        }
        catch (Exception) { }
    }

    /// <summary>
    /// 添加数据库填充服务
    /// </summary>
    private static IServiceCollection AddDataSeedServices(this IServiceCollection services)
    {
        // 注册数据库填充管理器
        services.AddScoped<DataSeedManager>();

        var assembliesToScan = new List<Assembly>();

        var currentAssembly = typeof(ServiceCollectionExtensions).Assembly;
        assembliesToScan.Add(currentAssembly);

        var entryAssembly = Assembly.GetEntryAssembly();
        if (entryAssembly != null && entryAssembly != currentAssembly)
        {
            assembliesToScan.Add(entryAssembly);
        }

        var allSeedServiceTypes = new List<Type>();

        foreach (var assembly in assembliesToScan)
        {
            var seedServiceTypes = assembly
                .GetTypes()
                .Where(t =>
                    t.IsClass && !t.IsAbstract && typeof(IDataSeedService).IsAssignableFrom(t)
                )
                .ToList();

            allSeedServiceTypes.AddRange(seedServiceTypes);
        }

        foreach (var serviceType in allSeedServiceTypes)
        {
            services.AddScoped(typeof(IDataSeedService), serviceType);
        }

        return services;
    }
}
