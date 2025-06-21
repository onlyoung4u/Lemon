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
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "Database": "Host=localhost;Port=5432;Username=postgres;Password=your-password;Database=your-database;",
    "Redis": "localhost:6379"
  },
  "Database": {
    "Type": "MySQL",
    "ConnectionPool": false,
    "AutoSyncStructure": true
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

## 许可证

本项目采用 MIT 许可证。
