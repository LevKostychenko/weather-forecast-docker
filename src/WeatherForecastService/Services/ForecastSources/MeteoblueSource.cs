using Microsoft.Extensions.Options;
using System.Text.Json.Serialization;
using WeatherForecastService.Constants;
using WeatherForecastService.Exceptions;
using WeatherForecastService.Models;
using WeatherForecastService.Models.Options;
using WeatherForecastService.Services.Abstractions;

namespace WeatherForecastService.Services.ForecastSources
{
    internal class MeteoblueSource(
        IHttpClientFactory httpClientFactory, 
        IGeocodingService geocodingService, 
        IOptions<MeteoblueSourceOptions> options) : IForecastSource
    {
        public string Name => ForecastSourceConstants.SourceName.Meteoblue;

        public async Task<WeatherForecast> GetForecastAsync(DateOnly date, string city, string country, CancellationToken token = default)
        {
            var coordinates = await geocodingService.GetCoordinatesAsync(city, country, token);

            var request = new HttpRequestMessage(HttpMethod.Get, $"packages/basic-day?" +
                $"lat={coordinates.Latitude}&" +
                $"lon={coordinates.Longitude}&" +
                $"apikey={options.Value.ApiKey}");

            var client = httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(options.Value.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(options.Value.TimeoutSeconds);
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var meteoblueResponse = await response.Content.ReadFromJsonAsync<MeteoblueResponse>(cancellationToken: token)
                ?? throw new ForecastSourceException(Name, "Meteoblue response is null");

            var convertedResult = meteoblueResponse.DayData.Time.Select((time, index) =>
            {
                var maxTemp = meteoblueResponse.DayData.TemperatureMax.ElementAtOrDefault(index);
                var minTemp = meteoblueResponse.DayData.TemperatureMin.ElementAtOrDefault(index);

                return new
                {
                    Day = DateOnly.FromDateTime(time),
                    AirTemperatureMax = MathF.Round(maxTemp, 2),
                    AirTemperatureMin = MathF.Round(minTemp, 2),
                };
            })
            .FirstOrDefault(windyResponse => windyResponse.Day == date);

            return new WeatherForecast
            {
                MaxTemperatureCelsius = convertedResult?.AirTemperatureMax ?? float.MinValue,
                MinTemperatureCelsius = convertedResult?.AirTemperatureMin ?? float.MinValue
            };
        }

        private record MeteoblueResponse
        {
            [JsonPropertyName("data_day")]
            public MeteoblueResponseDayData DayData { get; set; } = new();
        }

        private record MeteoblueResponseDayData
        {
            public IEnumerable<DateTime> Time { get; set; } = [];
            [JsonPropertyName("temperature_min")]
            public IEnumerable<float> TemperatureMin { get; set; } = [];
            [JsonPropertyName("temperature_max")]
            public IEnumerable<float> TemperatureMax { get; set; } = [];
        }
    }
}
