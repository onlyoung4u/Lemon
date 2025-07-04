# LemonStudio

LemonStudio 是一个基于 ASP.NET Core 的快速开发框架，提供了完整的用户管理、权限控制、日志记录等功能。

## 快速开始

### 安装

```bash
dotnet new webapi -n Your.Project -controllers

cd Your.Project

dotnet add package LemonStudio
```

### 配置

在 `Program.cs` 中添加 LemonStudio 服务：

```csharp
using FluentValidation;
using Lemon.Services.Extensions;

var builder = WebApplication.CreateBuilder(args);

// 配置 Serilog 日志（可选）
builder.UseLemonSerilog();

// 添加 LemonStudio 服务
builder.Services.AddLemonServices(builder.Configuration);

// 添加 OpenApi
builder.Services.AddOpenApi();

// 添加验证器
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// 使用 LemonStudio 中间件
app.UseLemon(app.Environment.IsDevelopment());

app.MapControllers();

app.Run();
```

### 配置文件

创建 `appsettings.json` 配置文件：

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Serilog": {
    "EnableFileLogging": true,
    "EnableConsoleLogging": true,
    "LogFilePath": "logs/app-.log",
    "RollingInterval": "Day",
    "RetainedFileCountLimit": 30,
    "FileSizeLimitBytes": 10485760,
    "MinimumLevel": "Information",
    "MinimumLevelOverrides": {
      "Microsoft": "Warning",
      "Microsoft.AspNetCore": "Warning",
      "System": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "Redis": "localhost:6379"
  },
  "Databases": {
    "DefaultDatabase": "Default",
    "Connections": [
      {
        "Name": "Default",
        "Type": "PostgreSQL",
        "ConnectionString": "Host=localhost;Port=5432;Username=postgres;Password=your-password;Database=your-database;",
        "ConnectionPool": true,
        "AutoSyncStructure": true,
        "EnableMonitor": true
      },
      {
        "Name": "Secondary",
        "Type": "MySQL",
        "ConnectionString": "Server=localhost;User ID=root;Password=your-password;Database=secondary_db;",
        "ConnectionPool": true,
        "AutoSyncStructure": false,
        "EnableMonitor": false
      }
    ]
  },
  "Switch": {
    "Admin": true,
    "DataSeed": true
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:3000", "http://localhost:8080"]
  },
  "Jwt": [
    {
      "Name": "Admin",
      "SecretKey": "your-secret-key-here-must-be-at-least-32-characters",
      "Issuer": "Lemon.Api",
      "Audience": "Lemon.Client",
      "ExpiresInMinutes": 120,
      "SSO": false
    }
  ],
  "Response": {
    "CustomMessages": {
      "2001": "用户不存在",
      "2002": "密码错误",
      "2003": "用户已被禁用",
      "3001": "文件上传失败",
      "3002": "文件格式不支持",
      "3003": "文件大小超出限制",
      "4001": "业务规则验证失败",
      "4002": "数据同步失败",
      "4003": "第三方服务调用失败"
    }
  }
}
```

### 日志配置

LemonStudio 集成了 Serilog 日志框架，提供强大的日志记录功能。

#### 启用 Serilog

在 `Program.cs` 中调用 `UseLemonSerilog()` 即可启用：

```csharp
// 使用默认配置
builder.UseLemonSerilog();

// 或者自定义配置
builder.UseLemonSerilog(options =>
{
    options.LogFilePath = "/var/log/myapp/app-.log";
    options.MinimumLevel = "Debug";
    options.RetainedFileCountLimit = 60;
});
```

#### 在代码中使用日志

```csharp
public class MyController : ControllerBase
{
    private readonly ILogger<MyController> _logger;

    public MyController(ILogger<MyController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Get()
    {
        _logger.LogInformation("处理 GET 请求");

        // 结构化日志
        _logger.LogInformation("用户 {UserId} 执行了 {Action} 操作", 123, "查询");

        return Ok();
    }
}
```

#### 日志输出

- **控制台**: 开发环境实时查看
- **文件**: 生产环境持久化存储
- **Systemd Journal**: 通过 `journalctl` 命令查看

详细配置说明请参考 [Serilog 集成文档](Services/Logging/README.md)。

#### 多数据库使用方式

在业务服务中使用多数据库，可以继承 `MultiDatabaseBaseService`：

```csharp
using Lemon.Business.Base;
using Lemon.Services.Database;

public class MyService : MultiDatabaseBaseService
{
    public MyService(IFreeSql freeSql, IMultiDatabaseService multiDatabase)
        : base(freeSql, multiDatabase)
    {
    }

    public async Task<List<User>> GetUsersFromSecondaryDatabase()
    {
        // 获取指定数据库实例
        var secondaryDb = GetDatabase("Secondary");

        // 使用指定数据库进行操作
        return await secondaryDb.Select<User>().ToListAsync();
    }

    public async Task<List<User>> GetUsersFromDefaultDatabase()
    {
        // 使用默认数据库
        return await Db.Select<User>().ToListAsync();
    }
}
```

或者直接注入 `IMultiDatabaseService`：

```csharp
public class MyController : ControllerBase
{
    private readonly IMultiDatabaseService _multiDatabase;

    public MyController(IMultiDatabaseService multiDatabase)
    {
        _multiDatabase = multiDatabase;
    }

    [HttpGet]
    public async Task<IActionResult> GetData()
    {
        // 获取指定数据库
        var db = _multiDatabase.GetDatabase("Secondary");
        var data = await db.Select<MyEntity>().ToListAsync();

        return Success(data);
    }
}
```

## 许可证

本项目采用 MIT 许可证。
