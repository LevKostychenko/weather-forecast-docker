using WeatherForecastService.Models;

namespace WeatherForecastService.Services.Abstractions
{
    public interface IGeocodingService
    {
        Task<Coordinates> GetCoordinatesAsync(string city, string country, CancellationToken token = default);
    }
}
