using WeatherForecastService.Services.Abstractions;

namespace WeatherForecastService.Services
{
    internal class CountryCodeResolver(HttpClient http) : ICountryCodeResolver
    {
        public async Task<string?> ResolveCountryCodeAsync(string countryName, CancellationToken token = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"name/{countryName.ToLowerInvariant()}?fields=cca2");
            var response = await http.SendAsync(request, token);
            response.EnsureSuccessStatusCode();
            var countryData = await response.Content.ReadFromJsonAsync<IEnumerable<CountryData>>(cancellationToken: token);
            return countryData?.FirstOrDefault()?.Cca2;
        }

        private record CountryData
        {
            public string Cca2 { get; set; } = string.Empty;
        }
    }
}
