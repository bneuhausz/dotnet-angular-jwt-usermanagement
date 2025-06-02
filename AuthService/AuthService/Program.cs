using AuthService.BackgroundServices;
using AuthService.Data;
using AuthService.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AuthDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("AuthDb")));

builder.Services.AddSingleton<IPasswordHasher<object>, PasswordHasher<object>>();
builder.Services.AddSingleton<PasswordVerificationService>();

builder.Services.AddHostedService<RefreshTokenCleanupService>();

builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
