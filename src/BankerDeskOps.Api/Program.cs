using BankerDeskOps.Api.Middleware;
using BankerDeskOps.Api.Services;
using BankerDeskOps.Application;
using BankerDeskOps.Application.Interfaces;
using BankerDeskOps.Application.Services;
using BankerDeskOps.Domain.Entities;
using BankerDeskOps.Domain.Enums;
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
builder.Services.AddApplication();

// Add AI-powered loan processing services
builder.Services.AddScoped<ILoanValidationService, LoanValidationService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IAIAnalysisService, AIAnalysisService>();
builder.Services.AddScoped<IDecisionEngineService, DecisionEngineService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// Add gRPC service implementations
builder.Services.AddScoped<LoanServiceImpl>();
builder.Services.AddScoped<RetailAccountServiceImpl>();
builder.Services.AddScoped<BankClientServiceImpl>();
builder.Services.AddScoped<ContractServiceImpl>();
builder.Services.AddScoped<UserServiceImpl>();
builder.Services.AddScoped<TransactionServiceImpl>();

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

                                // Seed default admin user if no users exist
        if (!await dbContext.Users.AnyAsync())
        {
            dbContext.Users.Add(new User()
            {
                Id = Guid.NewGuid(),
                Username = "admin",
                Email = "admin@bankerdeskops.local",
                FirstName = "System",
                LastName = "Administrator",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Role = UserRole.Admin,
                Status = UserStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
            await dbContext.SaveChangesAsync();
        }

        // Seed default currencies if none exist
        if (!await dbContext.Currencies.AnyAsync())
        {
            var defaultCurrencies = new[]
            {
                new Currency { Id = Guid.NewGuid(), Code = "USD", Name = "US Dollar" },
                new Currency { Id = Guid.NewGuid(), Code = "EUR", Name = "Euro" },
                new Currency { Id = Guid.NewGuid(), Code = "GBP", Name = "British Pound" },
                new Currency { Id = Guid.NewGuid(), Code = "UAH", Name = "Ukrainian Hryvnia" }
            };
            dbContext.Currencies.AddRange(defaultCurrencies);
            await dbContext.SaveChangesAsync();
        }
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
app.MapGrpcService<BankClientServiceImpl>();
app.MapGrpcService<ContractServiceImpl>();
app.MapGrpcService<UserServiceImpl>();
app.MapGrpcService<TransactionServiceImpl>();

// Map REST controllers (kept for backward compatibility)
app.MapControllers();

app.Run();

// Make Program class accessible for WebApplicationFactory in integration tests
namespace BankerDeskOps.Api
{
    public partial class Program { }
}

