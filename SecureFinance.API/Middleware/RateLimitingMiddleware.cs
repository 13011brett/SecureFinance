using System.Collections.Concurrent;
using System.Net;

namespace SecureFinance.API.Middleware
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitingMiddleware> _logger;
        
        // Use thread-safe in-memory storage
        private static readonly ConcurrentDictionary<string, List<DateTime>> _requestLog = new();

        public RateLimitingMiddleware(
            RequestDelegate next,
            ILogger<RateLimitingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var clientId = GetClientIdentifier(context);

            try
            {
                // Simple in-memory rate limiting (100 requests per minute)
                var now = DateTime.UtcNow;
                var oneMinuteAgo = now.AddMinutes(-1);

                // Get or create request list for this client
                var requests = _requestLog.GetOrAdd(clientId, _ => new List<DateTime>());

                // Thread-safe cleanup and counting
                bool rateLimitExceeded = false;
                int remaining = 0;

                lock (requests)
                {
                    // Remove old requests
                    requests.RemoveAll(time => time < oneMinuteAgo);

                    // Check rate limit
                    if (requests.Count >= 100)
                    {
                        rateLimitExceeded = true;
                        remaining = 0;
                    }
                    else
                    {
                        // Add current request
                        requests.Add(now);
                        remaining = Math.Max(0, 100 - requests.Count);
                    }
                }

                // Handle rate limit exceeded (outside of lock)
                if (rateLimitExceeded)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                    context.Response.Headers["X-RateLimit-Limit"] = "100";
                    context.Response.Headers["X-RateLimit-Remaining"] = "0";
                    context.Response.Headers["X-RateLimit-Reset"] = DateTimeOffset.UtcNow.AddMinutes(1).ToUnixTimeSeconds().ToString();

                    await context.Response.WriteAsync("Rate limit exceeded. Try again later.");
                    return;
                }

                // Add rate limit headers for successful requests
                context.Response.Headers["X-RateLimit-Limit"] = "100";
                context.Response.Headers["X-RateLimit-Remaining"] = remaining.ToString();
                context.Response.Headers["X-RateLimit-Reset"] = DateTimeOffset.UtcNow.AddMinutes(1).ToUnixTimeSeconds().ToString();

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