using WeatherForecastService.Models;
using WeatherForecastService.Services.Abstractions;

namespace WeatherForecastService.Services
{
    internal class ForecastService(
        ICacheService cacheService,
        ILogger<ForecastService> logger, 
        IForecastSourceManager sourceManager) : IForecastService
    {
        public async Task<AggregatedWeatherForecast> GetWeatherForecastAsync(string city, string country, DateOnly date, CancellationToken token = default)
        {
            var sources = sourceManager.GetForecastSources();
            logger.LogInformation(
                "Forecast sources registered: {Sources}",
                sources.Select(s => s.Name)
            );

            var tasks = sources.Select(source => GetForecastSafeAsync(source, date, city, country, token));
            var results = await Task.WhenAll(tasks);

            return new AggregatedWeatherForecast
            {
                City = city,
                Country = country,
                TimeStamp = DateTime.UtcNow,
                Forecasts = results
                    .Where(forecast => forecast.Success)
                    .ToDictionary(x => x.SourceName, x => x.Forecast)
            };
        }

        private async Task<WeatherForecastSourceResult> GetForecastSafeAsync(
            IForecastSource source,
            DateOnly date,
            string city,
            string country,
            CancellationToken token)
        {
            try
            {
                var cacheKey = CreateCacheKey(city, country, date, source.Name);
                var cachedResult = await cacheService.GetValueAsync<WeatherForecast>(cacheKey, token);
                if (cachedResult is not null)
                {
                    return new WeatherForecastSourceResult
                    {
                        Forecast = cachedResult,
                        SourceName = source.Name
                    };
                }

                var result = await source.GetForecastAsync(date, city, country, token);
                if (result is not null)
                {
                    await cacheService.SetValueAsync(cacheKey, result, TimeSpan.FromHours(24), token);
                }

                return new WeatherForecastSourceResult
                {
                    Forecast = result,
                    SourceName = source.Name
                };
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Forecast source {Source} failed", source.Name);
                return new WeatherForecastSourceResult
                {
                    SourceName = source.Name,
                };
            }
        }

        private static string CreateCacheKey(string city, string country, DateOnly day, string sourceName) =>
            $"WeatherForecast_{city.ToLowerInvariant()}_{country.ToLowerInvariant()}_{day}_{sourceName}";

        private record WeatherForecastSourceResult
        {
            public string SourceName { get; init; } = string.Empty;
            public WeatherForecast? Forecast { get; init; }
            public bool Success => Forecast is not null;
        }
    }
}
