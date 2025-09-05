namespace SecureFinance.Core.DTOs
{
    public class StockDataDto
    {
        public string Symbol { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal Change { get; set; }
        public decimal ChangePercent { get; set; }
        public long Volume { get; set; }
        public DateTime LastUpdated { get; set; }
        public string Source { get; set; } = string.Empty;
    }

    public class CryptoDataDto
    {
        public string Symbol { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal Change24h { get; set; }
        public decimal ChangePercent24h { get; set; }
        public decimal MarketCap { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class ExchangeRateDto
    {
        public string FromCurrency { get; set; } = string.Empty;
        public string ToCurrency { get; set; } = string.Empty;
        public decimal Rate { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class EconomicIndicatorDto
    {
        public string SeriesId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public string Units { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}