using Microsoft.Extensions.Caching.Memory;
using WeatherForecastService.Services.Abstractions;

namespace WeatherForecastService.Services
{
    internal class MemoryCacheService(IMemoryCache cache) : ICacheService
    {
        public Task<T?> GetValueAsync<T>(string key, CancellationToken token = default)
        {
            cache.TryGetValue(key, out T? value);
            return Task.FromResult(value);
        }

        public Task SetValueAsync<T>(string key, T value, TimeSpan ttl, CancellationToken token = default)
        {
            cache.Set(key, value, ttl);
            return Task.CompletedTask;
        }
    }
}
