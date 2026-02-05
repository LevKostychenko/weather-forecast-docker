namespace WeatherForecastService.Services.Abstractions
{
    public interface ICountryCodeResolver
    {
        Task<string?> ResolveCountryCodeAsync(string countryName, CancellationToken token = default);
    }
}
