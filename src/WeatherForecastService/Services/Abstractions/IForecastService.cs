using WeatherForecastService.Models;

namespace WeatherForecastService.Services.Abstractions
{
    public interface IForecastService
    {
        Task<AggregatedWeatherForecast> GetWeatherForecastAsync(string city, string country, DateOnly date, CancellationToken token = default);
    }
}
