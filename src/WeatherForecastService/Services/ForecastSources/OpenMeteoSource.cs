using OpenMeteo;
using WeatherForecastService.Constants;
using WeatherForecastService.Exceptions;
using WeatherForecastService.Models;
using WeatherForecastService.Services.Abstractions;

namespace WeatherForecastService.Services.ForecastSources
{
    internal class OpenMeteoSource(IGeocodingService geocodingService) : IForecastSource
    {
        public string Name => ForecastSourceConstants.SourceName.OpenMeteo;

        private readonly DailyOptions _dailyOptions = new(
            [
                DailyOptionsParameter.temperature_2m_max,
                DailyOptionsParameter.temperature_2m_min,
            ]);

        public async Task<Models.WeatherForecast> GetForecastAsync(DateOnly date, string city, string country, CancellationToken token = default)
        {
            var coordinates = await geocodingService.GetCoordinatesAsync(city, country, token);

            var dayString = date.ToString("yyyy-MM-dd");
            var options = GetBasicOptions(coordinates);
            options.Start_date = dayString;
            options.End_date = dayString;

            var dayForecast = await GetWeatherForecastAsync(options, date);

            return new Models.WeatherForecast
            {
                MaxTemperatureCelsius = dayForecast.AirTemperatureMax,
                MinTemperatureCelsius = dayForecast.AirTemperatureMin,
            };
        }

        private WeatherForecastOptions GetBasicOptions(Coordinates coordinates)
            => new()
            {
                Longitude = (float)coordinates.Longitude,
                Latitude = (float)coordinates.Latitude,
                Daily = _dailyOptions,
            };

        private async Task<DayWeatherForecast> GetWeatherForecastAsync(WeatherForecastOptions options, DateOnly date)
        {
            var client = new OpenMeteoClient();
            var response = await client.QueryAsync(options);
            return response is null 
                ? throw new ForecastSourceException(Name, "response is null") 
                : MapResult(response, date);
        }

        private static DayWeatherForecast MapResult(OpenMeteo.WeatherForecast response, DateOnly date)
        {
            return new DayWeatherForecast
            {
                Day = date,
                AirTemperatureMax = response.Daily?.Temperature_2m_max?[0] ?? float.MinValue,
                AirTemperatureMin = response.Daily?.Temperature_2m_min?[0] ?? float.MinValue,
            };
        }

        private record DayWeatherForecast
        {
            public DateOnly Day { get; set; }

            public float AirTemperatureMax { get; set; }
            public float AirTemperatureMin { get; set; }
        }
    }
}
