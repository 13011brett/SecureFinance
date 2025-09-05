using Microsoft.AspNetCore.Mvc;
using SecureFinance.Core.DTOs;
using SecureFinance.Infrastructure.Services;
using System.Diagnostics;

namespace SecureFinance.API.Controllers
{
    [ApiController]
    [Route("api/rates")]
    public class ExchangeRateController : ControllerBase
    {
        private readonly IFinancialDataService _financialDataService;
        private readonly ILogger<ExchangeRateController> _logger;

        public ExchangeRateController(
            IFinancialDataService financialDataService,
            ILogger<ExchangeRateController> logger)
        {
            _financialDataService = financialDataService;
            _logger = logger;
        }

        /// <summary>
        /// Get exchange rate between two currencies
        /// </summary>
        [HttpGet("currency/{from}/{to}")]
        public async Task<ActionResult<ApiResponseDto<ExchangeRateDto>>> GetExchangeRate(string from, string to)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var exchangeRate = await _financialDataService.GetExchangeRateAsync(from, to);

                stopwatch.Stop();

                if (exchangeRate == null)
                {
                    return NotFound(new ApiResponseDto<ExchangeRateDto>
                    {
                        Success = false,
                        ErrorMessage = $"Exchange rate not found for {from} to {to}",
                        ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds
                    });
                }

                return Ok(new ApiResponseDto<ExchangeRateDto>
                {
                    Success = true,
                    Data = exchangeRate,
                    ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving exchange rate for {From} to {To}", from, to);

                return StatusCode(500, new ApiResponseDto<ExchangeRateDto>
                {
                    Success = false,
                    ErrorMessage = "Internal server error occurred",
                    ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds
                });
            }
        }
    }
}