using FluentValidation;
using Lemon.Sample.Services.Queue;
using Lemon.Sample.Services.Schedule;
using Lemon.Services.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLemonServices(builder.Configuration);

// 添加队列服务
var queueOptions = builder.Configuration.GetSection("Queue").Get<QueueOptions>();

if (
    queueOptions is not null
    && !string.IsNullOrEmpty(queueOptions.Database)
    && !string.IsNullOrEmpty(queueOptions.Redis)
)
{
    builder.Services.AddCap(x =>
    {
        x.UsePostgreSql(queueOptions.Database);
        x.UseRedis(queueOptions.Redis);
        x.FailedRetryCount = queueOptions.FailedRetryCount;
        x.DefaultGroupName = queueOptions.DefaultGroupName;
    });

    // 注册队列消费者
    builder.Services.AddQueueConsumers();
}

// 添加调度器服务
builder.Services.AddSchedulers();

builder.Services.AddOpenApi();

// 添加验证器
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseLemon(app.Environment.IsDevelopment());

// 配置定时任务调度
app.UseSchedulers();

app.MapControllers();

app.Run();
