using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureFinance.Core.DTOs;
using SecureFinance.Infrastructure.Data;
using System.Diagnostics;

namespace SecureFinance.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AdminController> _logger;

        public AdminController(AppDbContext context, ILogger<AdminController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get API usage statistics
        /// </summary>
        [HttpGet("usage")]
        public async Task<ActionResult<ApiResponseDto<UsageStatsDto>>> GetUsageStats(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var start = startDate ?? DateTime.UtcNow.AddDays(-7);
                var end = endDate ?? DateTime.UtcNow;

                var requests = await _context.ApiRequests
                    .Where(r => r.RequestedAt >= start && r.RequestedAt <= end)
                    .ToListAsync();

                var stats = new UsageStatsDto
                {
                    TotalRequests = requests.Count,
                    CachedRequests = requests.Count(r => r.WasCached),
                    AverageResponseTime = requests.Any() ? requests.Average(r => r.ResponseTimeMs) : 0,
                    ErrorCount = requests.Count(r => !r.IsSuccess),
                    PeriodStart = start,
                    PeriodEnd = end,
                    RequestsByEndpoint = requests
                        .GroupBy(r => r.Endpoint)
                        .ToDictionary(g => g.Key, g => g.Count())
                };

                stopwatch.Stop();

                return Ok(new ApiResponseDto<UsageStatsDto>
                {
                    Success = true,
                    Data = stats,
                    ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving usage statistics");

                return StatusCode(500, new ApiResponseDto<UsageStatsDto>
                {
                    Success = false,
                    ErrorMessage = "Internal server error occurred",
                    ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds
                });
            }
        }

        /// <summary>
        /// Get system health status
        /// </summary>
        [HttpGet("health")]
        public async Task<ActionResult<ApiResponseDto<object>>> GetHealth()
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // Check database connectivity
                var canConnectToDb = await _context.Database.CanConnectAsync();

                // Check data sources status
                var dataSources = await _context.DataSources
                    .Where(ds => ds.IsActive)
                    .Select(ds => new { ds.Name, ds.IsActive, ds.LastHealthCheck })
                    .ToListAsync();

                var healthStatus = new
                {
                    Status = canConnectToDb ? "Healthy" : "Unhealthy",
                    Database = canConnectToDb ? "Connected" : "Disconnected",
                    DataSources = dataSources,
                    Timestamp = DateTime.UtcNow,
                    Uptime = stopwatch.Elapsed
                };

                stopwatch.Stop();

                return Ok(new ApiResponseDto<object>
                {
                    Success = true,
                    Data = healthStatus,
                    ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking system health");

                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    ErrorMessage = "Health check failed",
                    ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds
                });
            }
        }
    }
}
