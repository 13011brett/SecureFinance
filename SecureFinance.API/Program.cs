using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SecureFinance.API.Middleware;
using SecureFinance.Core.Services;
using SecureFinance.Infrastructure.Data;
using SecureFinance.Infrastructure.Services;
using Serilog;
using StackExchange.Redis;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();

// Configure Entity Framework
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(provider =>
{
    var connectionString = builder.Configuration.GetConnectionString("Redis");
    return ConnectionMultiplexer.Connect(connectionString);
});

// Register services
builder.Services.AddSingleton<ICacheService, RedisCacheService>();
builder.Services.AddScoped<IEncryptionService, HybridEncryptionService>();
builder.Services.AddScoped<IFinancialDataService, FinancialDataService>();

// Configure HttpClient
builder.Services.AddHttpClient<IFinancialDataService, FinancialDataService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("User-Agent", "SecureFinance-API/1.0");
});

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SecureFinance API",
        Version = "v1",
        Description = "A secure financial data aggregation API with enterprise-grade encryption",
        Contact = new OpenApiContact
        {
            Name = "Brett Scarlett",
            Email = "brett@scarletttech.com",
            Url = new Uri("https://github.com/13011brett")
        }
    });

    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Name = "X-API-Key",
        Description = "API Key for authentication"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                }
            },
            new string[] {}
        }
    });

    // Include XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Add health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>()
    .AddCheck("redis", () =>
    {
        try
        {
            var redis = builder.Services.BuildServiceProvider().GetService<IConnectionMultiplexer>();
            return redis?.IsConnected == true ?
                Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy() :
                Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Unhealthy();
        }
        catch
        {
            return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Unhealthy();
        }
    });

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "SecureFinance API v1");
        c.RoutePrefix = string.Empty; // Swagger at root
    });
}

//app.UseHttpsRedirection();

// Add custom middleware
app.UseMiddleware<ApiLoggingMiddleware>();
app.UseMiddleware<RateLimitingMiddleware>();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health");

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        context.Database.EnsureCreated();
        Log.Information("Database initialized successfully");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error initializing database");
    }
}

Log.Information("SecureFinance API starting up...");

app.Run();