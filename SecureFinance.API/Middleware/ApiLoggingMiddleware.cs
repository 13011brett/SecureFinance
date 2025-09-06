using Microsoft.EntityFrameworkCore;
using SecureFinance.Core.Models;
using SecureFinance.Infrastructure.Data;
using System.Diagnostics;

namespace SecureFinance.API.Middleware
{
    public class ApiLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ApiLoggingMiddleware> _logger;

        public ApiLoggingMiddleware(RequestDelegate next, ILogger<ApiLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Only log API endpoints, skip health checks, swagger, etc.
            if (!ShouldLogRequest(context.Request.Path))
            {
                await _next(context);
                return;
            }

            var stopwatch = Stopwatch.StartNew();
            var isSuccess = true;
            string? errorMessage = null;

            try
            {
                await _next(context);
                isSuccess = context.Response.StatusCode >= 200 && context.Response.StatusCode < 300;
            }
            catch (Exception ex)
            {
                isSuccess = false;
                errorMessage = ex.Message;
                _logger.LogError(ex, "Error processing request {Path}", context.Request.Path);
                throw;
            }
            finally
            {
                stopwatch.Stop();

                // Log after the response to avoid delaying it
                await LogRequestAsync(context, (int)stopwatch.ElapsedMilliseconds, isSuccess, errorMessage);
            }
        }

        private async Task LogRequestAsync(HttpContext context, int responseTimeMs, bool isSuccess, string? errorMessage)
        {
            try
            {
                var dbContext = context.RequestServices.GetRequiredService<AppDbContext>();
                
                // Get or create default user
                var userId = await GetOrCreateDefaultUserAsync(dbContext);
                
                var apiRequest = new ApiRequest
                {
                    UserId = userId,
                    DataSourceId = GetDataSourceId(context.Request.Path),
                    Endpoint = GetNormalizedEndpoint(context.Request.Path),
                    ResponseTimeMs = responseTimeMs,
                    WasCached = false, // We'll improve this later
                    RequestedAt = DateTime.UtcNow,
                    IsSuccess = isSuccess,
                    ErrorMessage = errorMessage
                };

                dbContext.ApiRequests.Add(apiRequest);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log API request");
            }
        }

        private async Task<int> GetOrCreateDefaultUserAsync(AppDbContext dbContext)
        {
            const string defaultEmail = "demo@securefinance.com";
            
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == defaultEmail);
            if (user == null)
            {
                user = new User
                {
                    Email = defaultEmail,
                    ApiKey = "sf_demo_key_for_portfolio_showcase",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    LastActive = DateTime.UtcNow
                };
                
                dbContext.Users.Add(user);
                await dbContext.SaveChangesAsync();
            }
            
            return user.Id;
        }

        private int GetDataSourceId(string path)
        {
            if (path.Contains("/api/stock"))
                return 1; // Alpha Vantage
            if (path.Contains("/api/crypto"))
                return 2; // CoinGecko
            if (path.Contains("/api/rates"))
                return 4; // Exchange Rates API
            
            return 1; // Default to Alpha Vantage
        }

        private string GetNormalizedEndpoint(string path)
        {
            if (path.StartsWith("/api/stock/"))
                return "/api/stock/*";
            if (path.StartsWith("/api/crypto/"))
                return "/api/crypto/*";
            if (path.StartsWith("/api/rates/"))
                return "/api/rates/*";
            
            return path;
        }

        private bool ShouldLogRequest(string path)
        {
            var pathsToLog = new[]
            {
                "/api/stock",
                "/api/crypto", 
                "/api/rates"
            };

            return pathsToLog.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase));
        }
    }
}