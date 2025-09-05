using SecureFinance.Core.DTOs;

namespace SecureFinance.Infrastructure.Services
{
    public interface IFinancialDataService
    {
        Task<StockDataDto?> GetStockDataAsync(string symbol);
        Task<CryptoDataDto?> GetCryptoDataAsync(string symbol);
        Task<ExchangeRateDto?> GetExchangeRateAsync(string fromCurrency, string toCurrency);
        Task<List<EconomicIndicatorDto>> GetEconomicIndicatorsAsync(List<string> seriesIds);
    }
}