using SecureFinance.Core.Services;
using System.Net;

namespace SecureFinance.API.Middleware
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ICacheService _cacheService;
        private readonly ILogger<RateLimitingMiddleware> _logger;

        public RateLimitingMiddleware(
            RequestDelegate next,
            ICacheService cacheService,
            ILogger<RateLimitingMiddleware> logger)
        {
            _next = next;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var clientId = GetClientIdentifier(context);
            var rateLimitKey = $"rate_limit:{clientId}";

            try
            {
                // Allow 100 requests per minute per client
                var requestCount = await _cacheService.IncrementAsync(rateLimitKey, 60);

                if (requestCount > 100)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                    context.Response.Headers.Add("X-RateLimit-Limit", "100");
                    context.Response.Headers.Add("X-RateLimit-Remaining", "0");
                    context.Response.Headers.Add("X-RateLimit-Reset", DateTimeOffset.UtcNow.AddMinutes(1).ToUnixTimeSeconds().ToString());

                    await context.Response.WriteAsync("Rate limit exceeded. Try again later.");
                    return;
                }

                // Add rate limit headers
                context.Response.Headers.Add("X-RateLimit-Limit", "100");
                context.Response.Headers.Add("X-RateLimit-Remaining", Math.Max(0, 100 - requestCount).ToString());
                context.Response.Headers.Add("X-RateLimit-Reset", DateTimeOffset.UtcNow.AddMinutes(1).ToUnixTimeSeconds().ToString());

                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in rate limiting middleware");
                await _next(context);
            }
        }

        private string GetClientIdentifier(HttpContext context)
        {
            // Try to get API key from header first
            if (context.Request.Headers.TryGetValue("X-API-Key", out var apiKey))
            {
                return apiKey.ToString();
            }

            // Fall back to IP address
            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }
    }
}