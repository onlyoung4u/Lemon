# DotNetCore.CAP 使用文档

## 概述

CAP 是一个基于 .NET Standard 的 C# 库，用于处理分布式事务中的数据一致性问题，它具有 EventBus 的功能，同时也是一个在微服务或者 SOA 系统中解决分布式事务问题的一个框架。

## 安装

### 1. 安装核心包

```bash
dotnet add package DotNetCore.CAP
```

### 2. 安装消息队列传输包（选择其一）

#### Redis Streams（推荐）

```bash
dotnet add package DotNetCore.CAP.RedisStreams
```

#### RabbitMQ

```bash
dotnet add package DotNetCore.CAP.RabbitMQ
```

#### Kafka

```bash
dotnet add package DotNetCore.CAP.Kafka
```

#### Azure Service Bus

```bash
dotnet add package DotNetCore.CAP.AzureServiceBus
```

#### Amazon SQS

```bash
dotnet add package DotNetCore.CAP.AmazonSQS
```

#### Redis Streams

```bash
dotnet add package DotNetCore.CAP.RedisStreams
```

### 3. 安装数据库存储包（选择其一）

#### PostgreSQL（推荐）

```bash
dotnet add package DotNetCore.CAP.PostgreSql
```

#### SQL Server

```bash
dotnet add package DotNetCore.CAP.SqlServer
```

#### MySQL

```bash
dotnet add package DotNetCore.CAP.MySql
```

#### MongoDB

```bash
dotnet add package DotNetCore.CAP.MongoDB
```

#### In-Memory（仅用于开发测试）

```bash
dotnet add package DotNetCore.CAP.InMemoryStorage
```

## 配置和注册服务

### 1. 在 Program.cs 中注册 CAP 服务

```csharp
using DotNetCore.CAP;

var builder = WebApplication.CreateBuilder(args);

// 添加 CAP 服务
builder.Services.AddCap(x =>
{
    // 使用 PostgreSQL 作为存储
    x.UsePostgreSql(opt =>
    {
        opt.ConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        opt.Schema = "cap";
    });

    // 使用 Redis Streams 作为消息传输
    x.UseRedis(opt =>
    {
        opt.Configuration = "localhost:6379";
        opt.StreamEntriesCount = 10;
        opt.ConnectionPoolSize = 10;
    });

    // 可选：配置其他选项
    x.DefaultGroupName = "lemon-studio";
    x.FailedRetryCount = 3;
});

// 其他服务注册...
builder.Services.AddControllers();

var app = builder.Build();

// 其他中间件配置...
app.MapControllers();

app.Run();
```

### 2. 不同消息队列的配置示例

#### Redis Streams 配置（推荐）

```csharp
x.UseRedis(opt =>
{
    opt.Configuration = "localhost:6379";
    opt.StreamEntriesCount = 10;
    opt.ConnectionPoolSize = 10;
});
```

#### RabbitMQ 配置

```csharp
x.UseRabbitMQ(opt =>
{
    opt.HostName = "localhost";
    opt.Port = 5672;
    opt.UserName = "guest";
    opt.Password = "guest";
    opt.VirtualHost = "/";
    opt.ExchangeName = "cap.default.exchange";
});
```

#### Kafka 配置

```csharp
x.UseKafka(opt =>
{
    opt.Servers = "localhost:9092";
    opt.ConnectionPoolSize = 10;
});
```

### 3. 不同数据库的配置示例

#### PostgreSQL 配置（推荐）

```csharp
x.UsePostgreSql(opt =>
{
    opt.ConnectionString = "Host=localhost;Database=test;Username=postgres;Password=123456;";
    opt.Schema = "cap";
});
```

#### MySQL 配置

```csharp
x.UseMySql(opt =>
{
    opt.ConnectionString = "Server=localhost;Database=test;Uid=root;Pwd=123456;";
    opt.TableNamePrefix = "cap";
});
```

## 投递消息

### 1. 在 Controller 或 Service 中投递消息

```csharp
using DotNetCore.CAP;

[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly ICapPublisher _capPublisher;

    public OrderController(ICapPublisher capPublisher)
    {
        _capPublisher = capPublisher;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
    {
        // 创建订单逻辑...
        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = dto.UserId,
            Amount = dto.Amount,
            Status = OrderStatus.Created
        };

        // 投递消息到指定主题
        await _capPublisher.PublishAsync("order.created", order);

        return Ok(order);
    }
}
```

### 2. 延迟投递消息

```csharp
// 延迟 30 秒投递
await _capPublisher.PublishDelayAsync(TimeSpan.FromSeconds(30), "order.reminder", order);

// 指定具体时间投递
await _capPublisher.PublishDelayAsync(DateTime.Now.AddMinutes(10), "order.timeout", order);
```

## 消费消息

### 1. 创建消费者服务

```csharp
using DotNetCore.CAP;

public class OrderEventHandler : ICapSubscribe
{
    private readonly ILogger<OrderEventHandler> _logger;
    private readonly IEmailService _emailService;

    public OrderEventHandler(ILogger<OrderEventHandler> logger, IEmailService emailService)
    {
        _logger = logger;
        _emailService = emailService;
    }

    [CapSubscribe("order.created")]
    public async Task HandleOrderCreated(Order order)
    {
        _logger.LogInformation("处理订单创建事件: {OrderId}", order.Id);

        // 发送确认邮件
        await _emailService.SendOrderConfirmationAsync(order);

        _logger.LogInformation("订单创建事件处理完成: {OrderId}", order.Id);
    }

    [CapSubscribe("order.cancelled")]
    public async Task HandleOrderCancelled(Order order)
    {
        _logger.LogInformation("处理订单取消事件: {OrderId}", order.Id);

        // 退款逻辑
        await ProcessRefundAsync(order);

        // 发送取消通知
        await _emailService.SendOrderCancellationAsync(order);
    }

    private async Task ProcessRefundAsync(Order order)
    {
        // 退款处理逻辑
        await Task.Delay(1000); // 模拟异步操作
    }
}
```

### 2. 指定消费者组

```csharp
public class PaymentEventHandler : ICapSubscribe
{
    [CapSubscribe("order.created", Group = "payment-service")]
    public async Task ProcessPayment(Order order)
    {
        // 支付处理逻辑
        await Task.Delay(2000);
        Console.WriteLine($"处理订单 {order.Id} 的支付");
    }
}

public class InventoryEventHandler : ICapSubscribe
{
    [CapSubscribe("order.created", Group = "inventory-service")]
    public async Task UpdateInventory(Order order)
    {
        // 库存更新逻辑
        await Task.Delay(1000);
        Console.WriteLine($"更新订单 {order.Id} 的库存");
    }
}
```

### 3. 消费者异常处理

```csharp
public class OrderEventHandler : ICapSubscribe
{
    [CapSubscribe("order.created")]
    public async Task HandleOrderCreated(Order order, CancellationToken cancellationToken)
    {
        try
        {
            // 处理逻辑
            await ProcessOrderAsync(order, cancellationToken);
        }
        catch (Exception ex)
        {
            // 记录错误日志
            _logger.LogError(ex, "处理订单创建事件失败: {OrderId}", order.Id);

            // 重新抛出异常，CAP 会进行重试
            throw;
        }
    }
}
```

### 4. 注册消费者服务

确保在 Program.cs 中注册消费者服务：

```csharp
// 注册消费者服务
builder.Services.AddScoped<OrderEventHandler>();
builder.Services.AddScoped<PaymentEventHandler>();
builder.Services.AddScoped<InventoryEventHandler>();
```

## 高级功能

### 1. 自定义序列化

```csharp
builder.Services.AddCap(x =>
{
    x.UsePostgreSql(connectionString);
    x.UseRedis(redisOptions);

    // 使用自定义序列化器
    x.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});
```

### 2. 消息过滤器

```csharp
public class CustomCapFilter : SubscribeFilter
{
    public override async Task OnSubscribeExecutingAsync(ExecutingContext context)
    {
        // 订阅执行前
        Console.WriteLine($"开始执行订阅: {context.ConsumerDescriptor.TopicName}");
        await base.OnSubscribeExecutingAsync(context);
    }

    public override async Task OnSubscribeExecutedAsync(ExecutedContext context)
    {
        // 订阅执行后
        Console.WriteLine($"完成执行订阅: {context.ConsumerDescriptor.TopicName}");
        await base.OnSubscribeExecutedAsync(context);
    }
}

// 注册过滤器
builder.Services.AddCap(x =>
{
    x.UsePostgreSql(connectionString);
    x.UseRedis(redisOptions);
}).AddSubscribeFilter<CustomCapFilter>();
```

### 3. 消息监控面板

```csharp
// 添加 Dashboard
dotnet add package DotNetCore.CAP.Dashboard

// 在 Program.cs 中启用
app.UseCap();
```

访问 `http://localhost:5000/cap` 查看监控面板。

## 最佳实践

### 1. 消息设计原则

- 消息应该是不可变的
- 包含足够的上下文信息
- 使用明确的消息名称

### 2. 错误处理

- 实现幂等性消费
- 合理设置重试次数
- 记录详细的错误日志

### 3. 性能优化

- 合理设置连接池大小
- 使用批量投递（如果支持）
- 监控消息堆积情况

### 4. 数据模型示例

```csharp
public class Order
{
    public Guid Id { get; set; }
    public string UserId { get; set; }
    public decimal Amount { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

public enum OrderStatus
{
    Created,
    Paid,
    Shipped,
    Delivered,
    Cancelled
}

public class CreateOrderDto
{
    public string UserId { get; set; }
    public decimal Amount { get; set; }
}
```

## 故障排查

### 常见问题

1. **消息不被消费**

   - 检查消费者是否正确注册
   - 确认 Group 名称是否正确
   - 查看日志中的错误信息

2. **消息重复消费**

   - 实现消费幂等性
   - 检查消费者异常处理逻辑

3. **事务消息不一致**
   - 确保使用了 CAP 的事务扩展
   - 检查事务提交和回滚逻辑

## 配置文件示例

### appsettings.json 配置示例

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=MyApp;Username=postgres;Password=your_password;"
  },
  "Redis": {
    "Configuration": "localhost:6379",
    "InstanceName": "MyApp"
  },
  "CAP": {
    "DefaultGroup": "my-app",
    "FailedRetryCount": 3,
    "SucceedMessageExpiredAfter": 86400
  }
}
```

### 使用配置文件中的连接字符串

```csharp
builder.Services.AddCap(x =>
{
    // 从配置文件读取 PostgreSQL 连接字符串
    x.UsePostgreSql(opt =>
    {
        opt.ConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        opt.Schema = "cap";
    });

    // 从配置文件读取 Redis 连接字符串
    x.UseRedis(opt =>
    {
        opt.Configuration = builder.Configuration["Redis:Configuration"];
        opt.StreamEntriesCount = 10;
        opt.ConnectionPoolSize = 10;
    });

    // 从配置文件读取其他选项
    x.DefaultGroupName = builder.Configuration["CAP:DefaultGroup"];
    x.FailedRetryCount = builder.Configuration.GetValue<int>("CAP:FailedRetryCount");
});
```

通过以上配置和使用方式，您可以在 .NET 应用程序中成功集成和使用 DotNetCore.CAP 来处理分布式消息和事务。
