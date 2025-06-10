using FluentValidation;
using Lemon.Services.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLemonServices(builder.Configuration);

builder.Services.AddOpenApi();

// 添加验证器
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseLemon();

app.MapControllers();

app.Run();
