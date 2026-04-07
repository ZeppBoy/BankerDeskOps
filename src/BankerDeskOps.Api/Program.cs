using BankerDeskOps.Api.Middleware;
using BankerDeskOps.Api.Services;
using BankerDeskOps.Application.Interfaces;
using BankerDeskOps.Application.Services;
using BankerDeskOps.Infrastructure;
using BankerDeskOps.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Get connection string
var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Connection string 'Default' not found.");

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddGrpc();

// Add infrastructure services
builder.Services.AddInfrastructure(connectionString);

// Add application services
builder.Services.AddScoped<ILoanService, LoanService>();
builder.Services.AddScoped<IRetailAccountService, RetailAccountService>();

// Add gRPC service implementations
builder.Services.AddScoped<LoanServiceImpl>();
builder.Services.AddScoped<RetailAccountServiceImpl>();

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS if needed
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// Add global exception handling middleware
// Note: Temporarily disabled while investigating integration test failures
// app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

// Apply migrations and seed database on startup (but only for SQL Server, not SQLite tests)
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var connection = dbContext.Database.GetDbConnection();
    
    // Only run migrations for SQL Server, not for SQLite (used in tests)
    if (connection.GetType().Name != "SqliteConnection")
    {
        await dbContext.Database.MigrateAsync();
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();

// Map gRPC services
app.MapGrpcService<LoanServiceImpl>();
app.MapGrpcService<RetailAccountServiceImpl>();

// Map REST controllers (kept for backward compatibility)
app.MapControllers();

app.Run();

// Make Program class accessible for WebApplicationFactory in integration tests
namespace BankerDeskOps.Api
{
    public partial class Program { }
}

