using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SecureFinance.Core.Services;
using StackExchange.Redis;

namespace SecureFinance.Infrastructure.Services
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDatabase _database;
        private readonly IConnectionMultiplexer _connectionMultiplexer;

        public RedisCacheService(IConnectionMultiplexer connectionMultiplexer)
        {
            _connectionMultiplexer = connectionMultiplexer;
            _database = connectionMultiplexer.GetDatabase();
        }

        public async Task<T?> GetAsync<T>(string key) where T : class
        {
            var value = await _database.StringGetAsync(key);
            return value.HasValue ? JsonConvert.DeserializeObject<T>(value!) : null;
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
        {
            var serializedValue = JsonConvert.SerializeObject(value);
            await _database.StringSetAsync(key, serializedValue, expiration);
        }

        public async Task RemoveAsync(string key)
        {
            await _database.KeyDeleteAsync(key);
        }

        public async Task<bool> ExistsAsync(string key)
        {
            return await _database.KeyExistsAsync(key);
        }

        public async Task<long> IncrementAsync(string key, int expireInSeconds = 3600)
        {
            var count = await _database.StringIncrementAsync(key);
            if (count == 1)
            {
                await _database.KeyExpireAsync(key, TimeSpan.FromSeconds(expireInSeconds));
            }
            return count;
        }
    }
}