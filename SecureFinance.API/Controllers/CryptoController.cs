using Microsoft.AspNetCore.Mvc;
using SecureFinance.Core.DTOs;
using SecureFinance.Infrastructure.Services;
using System.Diagnostics;

namespace SecureFinance.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CryptoController : ControllerBase
    {
        private readonly IFinancialDataService _financialDataService;
        private readonly ILogger<CryptoController> _logger;

        public CryptoController(
            IFinancialDataService financialDataService,
            ILogger<CryptoController> logger)
        {
            _financialDataService = financialDataService;
            _logger = logger;
        }

        /// <summary>
        /// Get cryptocurrency data for a symbol
        /// </summary>
        [HttpGet("{symbol}")]
        public async Task<ActionResult<ApiResponseDto<CryptoDataDto>>> GetCrypto(string symbol)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var cryptoData = await _financialDataService.GetCryptoDataAsync(symbol);

                stopwatch.Stop();

                if (cryptoData == null)
                {
                    return NotFound(new ApiResponseDto<CryptoDataDto>
                    {
                        Success = false,
                        ErrorMessage = $"Cryptocurrency data not found for symbol: {symbol}",
                        ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds
                    });
                }

                return Ok(new ApiResponseDto<CryptoDataDto>
                {
                    Success = true,
                    Data = cryptoData,
                    ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving crypto data for {Symbol}", symbol);

                return StatusCode(500, new ApiResponseDto<CryptoDataDto>
                {
                    Success = false,
                    ErrorMessage = "Internal server error occurred",
                    ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds
                });
            }
        }
    }
}