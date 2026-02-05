namespace WeatherForecastService.Services.Abstractions
{
    public interface ICacheService
    {
        Task<T?> GetValueAsync<T>(string key, CancellationToken token = default);
        Task SetValueAsync<T>(string key, T value, TimeSpan ttl, CancellationToken token = default);
    }
}
