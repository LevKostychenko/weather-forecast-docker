using Microsoft.Extensions.Options;
using System.Text.Json.Serialization;
using WeatherForecastService.Constants;
using WeatherForecastService.Exceptions;
using WeatherForecastService.Models;
using WeatherForecastService.Models.Options;
using WeatherForecastService.Services.Abstractions;

namespace WeatherForecastService.Services.ForecastSources
{
    internal class WindySource(
        IHttpClientFactory httpClientFactory,
        IGeocodingService geocodingService, 
        IOptions<WindySourceOptions> options) : IForecastSource
    {
        public string Name => ForecastSourceConstants.SourceName.Windy;
        
        private const float KelvinToCelsius = 273.15f;
        private const string Model = "gfs";
        private readonly string[] _parameters = 
        [
           "temp",
        ];
        private readonly string[] _levels =
        [
          "surface",
        ];

        public async Task<WeatherForecast> GetForecastAsync(DateOnly date, string city, string country, CancellationToken token = default)
        {
            var coordinates = await geocodingService.GetCoordinatesAsync(city, country, token);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = JsonContent.Create(new
                {
                    lat = coordinates.Latitude,
                    lon = coordinates.Longitude,
                    model = Model,
                    key = options.Value.ApiKey,
                    parameters = _parameters,
                    levels = _levels
                }),
            };

            // TODO: Add polly
            var client = httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(options.Value.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(options.Value.TimeoutSeconds);
            var response = await client.SendAsync(request, token);
            response.EnsureSuccessStatusCode();

            var windyResponse = await response.Content.ReadFromJsonAsync<WindyResponse>(cancellationToken: token)
                ?? throw new ForecastSourceException(Name, "Windy response is null");

            var convertedResult = windyResponse.Ts.Select((timestamp, index) =>
            {
                var dto = DateTimeOffset.FromUnixTimeMilliseconds(timestamp);
                DateTime utc = dto.UtcDateTime;
                var temp = windyResponse.TempSurface.ElementAtOrDefault(index);

                return new
                {
                    Day = DateOnly.FromDateTime(utc),
                    AirTemperatureMax = MathF.Round(temp - KelvinToCelsius, 2),
                    AirTemperatureMin = MathF.Round(temp - KelvinToCelsius, 2),
                };
            })
            .FirstOrDefault(windyResponse => windyResponse.Day == date);

            return new WeatherForecast
            {
                MaxTemperatureCelsius = convertedResult?.AirTemperatureMax ?? float.MinValue,
                MinTemperatureCelsius = convertedResult?.AirTemperatureMin ?? float.MinValue,
            };
        }

        private record WindyResponse
        {
            public IEnumerable<long> Ts { get; set; } = [];
            public WindyUnit Units { get; set; } = new();
            [JsonPropertyName("temp-surface")]
            public IEnumerable<float> TempSurface { get; set; } = [];
        }

        private record WindyUnit
        {
            [JsonPropertyName("temp-surface")]
            public string TempSurface { get; set; } = string.Empty;
        }
    }
}
