using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using SecureFinance.Core.DTOs;
using SecureFinance.Core.Services;

namespace SecureFinance.Infrastructure.Services
{
    public class FinancialDataService : IFinancialDataService
    {
        private readonly HttpClient _httpClient;
        private readonly ICacheService _cacheService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<FinancialDataService> _logger;

        public FinancialDataService(
            HttpClient httpClient,
            ICacheService cacheService,
            IConfiguration configuration,
            ILogger<FinancialDataService> logger)
        {
            _httpClient = httpClient;
            _cacheService = cacheService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<StockDataDto?> GetStockDataAsync(string symbol)
        {
            var cacheKey = $"stock:{symbol.ToUpper()}";

            // Check cache first
            var cachedData = await _cacheService.GetAsync<StockDataDto>(cacheKey);
            if (cachedData != null)
            {
                return cachedData;
            }

            try
            {
                var apiKey = _configuration["ApiKeys:AlphaVantage"];
                var url = $"https://www.alphavantage.co/query?function=GLOBAL_QUOTE&symbol={symbol}&apikey={apiKey}";

                var response = await _httpClient.GetStringAsync(url);
                var json = JObject.Parse(response);

                var quote = json["Global Quote"];
                if (quote == null) return null;

                var stockData = new StockDataDto
                {
                    Symbol = symbol.ToUpper(),
                    Price = decimal.Parse(quote["05. price"]?.ToString() ?? "0"),
                    Change = decimal.Parse(quote["09. change"]?.ToString() ?? "0"),
                    ChangePercent = decimal.Parse(quote["10. change percent"]?.ToString()?.Replace("%", "") ?? "0"),
                    Volume = long.Parse(quote["06. volume"]?.ToString() ?? "0"),
                    LastUpdated = DateTime.Parse(quote["07. latest trading day"]?.ToString() ?? DateTime.UtcNow.ToString()),
                    Source = "Alpha Vantage"
                };

                // Cache for 30 seconds (stock data updates frequently)
                await _cacheService.SetAsync(cacheKey, stockData, TimeSpan.FromSeconds(30));

                return stockData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching stock data for {Symbol}", symbol);
                return null;
            }
        }

        public async Task<CryptoDataDto?> GetCryptoDataAsync(string symbol)
        {
            var cacheKey = $"crypto:{symbol.ToLower()}";

            // Check cache first
            var cachedData = await _cacheService.GetAsync<CryptoDataDto>(cacheKey);
            if (cachedData != null)
            {
                return cachedData;
            }

            try
            {
                var url = $"https://api.coingecko.com/api/v3/simple/price?ids={symbol.ToLower()}&vs_currencies=usd&include_24hr_change=true&include_market_cap=true";

                var response = await _httpClient.GetStringAsync(url);
                var json = JObject.Parse(response);

                var coinData = json[symbol.ToLower()];
                if (coinData == null) return null;

                var cryptoData = new CryptoDataDto
                {
                    Symbol = symbol.ToUpper(),
                    Name = symbol, // In real implementation, you'd get the full name
                    Price = decimal.Parse(coinData["usd"]?.ToString() ?? "0"),
                    Change24h = 0, // Calculate from price and change percent
                    ChangePercent24h = decimal.Parse(coinData["usd_24h_change"]?.ToString() ?? "0"),
                    MarketCap = decimal.Parse(coinData["usd_market_cap"]?.ToString() ?? "0"),
                    LastUpdated = DateTime.UtcNow
                };

                // Cache for 60 seconds
                await _cacheService.SetAsync(cacheKey, cryptoData, TimeSpan.FromSeconds(60));

                return cryptoData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching crypto data for {Symbol}", symbol);
                return null;
            }
        }

        public async Task<ExchangeRateDto?> GetExchangeRateAsync(string fromCurrency, string toCurrency)
        {
            var cacheKey = $"rate:{fromCurrency}:{toCurrency}";

            // Check cache first
            var cachedData = await _cacheService.GetAsync<ExchangeRateDto>(cacheKey);
            if (cachedData != null)
            {
                return cachedData;
            }

            try
            {
                var url = $"https://api.exchangerate-api.com/v4/latest/{fromCurrency.ToUpper()}";

                var response = await _httpClient.GetStringAsync(url);
                var json = JObject.Parse(response);

                var rates = json["rates"] as JObject;
                if (rates == null || !rates.ContainsKey(toCurrency.ToUpper())) return null;

                var rate = decimal.Parse(rates[toCurrency.ToUpper()]?.ToString() ?? "0");

                var exchangeRate = new ExchangeRateDto
                {
                    FromCurrency = fromCurrency.ToUpper(),
                    ToCurrency = toCurrency.ToUpper(),
                    Rate = rate,
                    LastUpdated = DateTime.UtcNow
                };

                // Cache for 5 minutes (exchange rates don't change that frequently)
                await _cacheService.SetAsync(cacheKey, exchangeRate, TimeSpan.FromMinutes(5));

                return exchangeRate;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching exchange rate for {From} to {To}", fromCurrency, toCurrency);
                return null;
            }
        }

        public async Task<List<EconomicIndicatorDto>> GetEconomicIndicatorsAsync(List<string> seriesIds)
        {
            var results = new List<EconomicIndicatorDto>();
            var apiKey = _configuration["ApiKeys:FRED"];

            foreach (var seriesId in seriesIds)
            {
                var cacheKey = $"economic:{seriesId}";

                // Check cache first
                var cachedData = await _cacheService.GetAsync<EconomicIndicatorDto>(cacheKey);
                if (cachedData != null)
                {
                    results.Add(cachedData);
                    continue;
                }

                try
                {
                    var url = $"https://api.stlouisfed.org/fred/series/observations?series_id={seriesId}&api_key={apiKey}&file_type=json&limit=1&sort_order=desc";

                    var response = await _httpClient.GetStringAsync(url);
                    var json = JObject.Parse(response);

                    var observations = json["observations"] as JArray;
                    if (observations == null || !observations.Any()) continue;

                    var latestObservation = observations[0];
                    var value = latestObservation["value"]?.ToString();

                    if (value == "." || string.IsNullOrEmpty(value)) continue; // FRED uses "." for missing data

                    var indicator = new EconomicIndicatorDto
                    {
                        SeriesId = seriesId,
                        Title = seriesId, // In real implementation, you'd fetch series info separately
                        Value = decimal.Parse(value),
                        Units = "Unknown", // Would be fetched from series info
                        Date = DateTime.Parse(latestObservation["date"]?.ToString() ?? DateTime.UtcNow.ToString()),
                        LastUpdated = DateTime.UtcNow
                    };

                    // Cache for 1 hour (economic data updates less frequently)
                    await _cacheService.SetAsync(cacheKey, indicator, TimeSpan.FromHours(1));

                    results.Add(indicator);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching economic data for series {SeriesId}", seriesId);
                }
            }

            return results;
        }
    }
}