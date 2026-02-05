using WeatherForecastService.Models;

namespace WeatherForecastService.Services.Abstractions
{
    public interface IForecastSource
    {
        string Name { get; }
        Task<WeatherForecast> GetForecastAsync(DateOnly date, string city, string country, CancellationToken token = default);
    }
}
