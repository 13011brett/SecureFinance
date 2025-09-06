namespace SecureFinance.Core.DTOs
{
    public class ApiResponseDto<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime Timestamp { get; set; }
        public bool FromCache { get; set; }
        public int ResponseTimeMs { get; set; }
    }

    public class UsageStatsDto
    {
        public int TotalRequests { get; set; }
        public int CachedRequests { get; set; }
        public double AverageResponseTime { get; set; }
        public int ErrorCount { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public Dictionary<string, int> RequestsByEndpoint { get; set; } = new();
    }
}