using System.Text.Json;
using WeatherForecastService.Exceptions;
using WeatherForecastService.Models;
using WeatherForecastService.Services.Abstractions;

namespace WeatherForecastService.Services
{
    internal class GeocodingService(
        HttpClient http, 
        ICountryCodeResolver countryCodeResolver,
        ICacheService cacheService) : IGeocodingService
    {
        private const int MaxResults = 1;

        public async Task<Coordinates> GetCoordinatesAsync(string city, string country, CancellationToken token = default)
        {            
            var cacheKey = CreateCacheKey(city, country);
            var cachedCoordinates = await cacheService.GetValueAsync<Coordinates>(cacheKey, token);
            if (cachedCoordinates is not null)
            {
                return cachedCoordinates;
            }

            var countryCode = await countryCodeResolver.ResolveCountryCodeAsync(country, token);
            if (string.IsNullOrEmpty(countryCode))
            {
                throw new GeocodingException($"Could not resolve country code for country: {country}");
            }
            // TODO: Add Polly
            var request = new HttpRequestMessage(HttpMethod.Get, $"?name={city}&countryCode={countryCode}&count={MaxResults}");
            var response = await http.SendAsync(request, token);

            response.EnsureSuccessStatusCode();
            var convertOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };        

            var geocodingData = await response.Content.ReadFromJsonAsync<GeocodingResponse>(
                options: convertOptions,
                cancellationToken: token);
            var firstResult = geocodingData?.Results?.FirstOrDefault() 
                ?? throw new GeocodingException($"Could not find coordinates for city: {city}, country: {countryCode}");

            await cacheService.SetValueAsync(cacheKey, firstResult, TimeSpan.FromHours(24), token);

            return firstResult;
        }

        private static string CreateCacheKey(string city, string country) =>
            $"GeocodingService_{city.ToLowerInvariant()}_{country.ToLowerInvariant()}";        

        private record GeocodingResponse
        {
            public IEnumerable<Coordinates> Results { get; set; } = [];
        }
    }
}
