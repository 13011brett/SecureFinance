using Microsoft.AspNetCore.Mvc;
using SecureFinance.Core.DTOs;
using SecureFinance.Core.Services;
using SecureFinance.Infrastructure.Services;
using System.Diagnostics;

namespace SecureFinance.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StockController : ControllerBase
    {
        private readonly IFinancialDataService _financialDataService;
        private readonly IEncryptionService _encryptionService;
        private readonly ILogger<StockController> _logger;

        public StockController(
            IFinancialDataService financialDataService,
            IEncryptionService encryptionService,
            ILogger<StockController> logger)
        {
            _financialDataService = financialDataService;
            _encryptionService = encryptionService;
            _logger = logger;
        }

        /// <summary>
        /// Get real-time stock data for a symbol
        /// </summary>
        [HttpGet("{symbol}")]
        public async Task<ActionResult<ApiResponseDto<StockDataDto>>> GetStock(string symbol)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var stockData = await _financialDataService.GetStockDataAsync(symbol);

                stopwatch.Stop();

                if (stockData == null)
                {
                    return NotFound(new ApiResponseDto<StockDataDto>
                    {
                        Success = false,
                        ErrorMessage = $"Stock data not found for symbol: {symbol}",
                        ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds
                    });
                }

                return Ok(new ApiResponseDto<StockDataDto>
                {
                    Success = true,
                    Data = stockData,
                    ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds,
                    FromCache = false // Would be determined by the service layer
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving stock data for {Symbol}", symbol);

                return StatusCode(500, new ApiResponseDto<StockDataDto>
                {
                    Success = false,
                    ErrorMessage = "Internal server error occurred while fetching stock data",
                    ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds
                });
            }
        }

        /// <summary>
        /// Get encrypted stock data (demonstrates encryption capability)
        /// </summary>
        [HttpGet("{symbol}/encrypted")]
        public async Task<ActionResult<ApiResponseDto<string>>> GetStockEncrypted(string symbol)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var stockData = await _financialDataService.GetStockDataAsync(symbol);

                if (stockData == null)
                {
                    return NotFound(new ApiResponseDto<string>
                    {
                        Success = false,
                        ErrorMessage = $"Stock data not found for symbol: {symbol}",
                        ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds
                    });
                }

                // For demo purposes, using userId = 1. In production, this would come from JWT
                var encryptedData = await _encryptionService.EncryptDataAsync(
                    System.Text.Json.JsonSerializer.Serialize(stockData),
                    userId: 1
                );

                stopwatch.Stop();

                return Ok(new ApiResponseDto<string>
                {
                    Success = true,
                    Data = encryptedData,
                    ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving encrypted stock data for {Symbol}", symbol);

                return StatusCode(500, new ApiResponseDto<string>
                {
                    Success = false,
                    ErrorMessage = "Internal server error occurred",
                    ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds
                });
            }
        }
    }
}