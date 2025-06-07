using Microsoft.EntityFrameworkCore;
using UserManagerApi.Services;
using UserManagerApi.Middlewares;
using Audit.Core;
using System.Text.Json;
using System.Diagnostics;
using UserManagerApi.Data.Audit;
using UserManagerApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureLogging(builder.Configuration);

builder.Services.ConfigureHttpLogging();
builder.Services.AddHttpContextAccessor();
builder.Services.ConfigureScopedServices();
builder.Services.ConfigureDatabaseContexts(builder.Configuration);
builder.Services.ConfigureSingletonServices();
builder.Services.ConfigureAuth(builder.Configuration);
builder.Services.ConfigureHostedServices();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.ConfigureAudit();

app.UseMiddleware<CorrelationIdMiddleware>();

app.UseHttpLogging();

app.UseAuthentication();

app.UseMiddleware<UserContextMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();
