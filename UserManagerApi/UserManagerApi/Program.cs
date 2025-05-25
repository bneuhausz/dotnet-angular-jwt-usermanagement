using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using UserManagerApi.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<UsersDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("UsersDb")));

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        var secret = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!);
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = "auth-service",
            ValidAudience = "user-manager-api",
            IssuerSigningKey = new SymmetricSecurityKey(secret),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
