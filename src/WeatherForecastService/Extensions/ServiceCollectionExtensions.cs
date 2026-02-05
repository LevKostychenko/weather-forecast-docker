using Microsoft.Extensions.Options;
using WeatherForecastService.Models.Options;
using WeatherForecastService.Services;
using WeatherForecastService.Services.Abstractions;
using WeatherForecastService.Services.ForecastSources;

namespace WeatherForecastService.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCache(this IServiceCollection services)
            => services
                .AddMemoryCache()
                .AddSingleton<ICacheService, MemoryCacheService>();

        public static IServiceCollection AddForecastSource<TImplementation>(this IServiceCollection services)
            where TImplementation : class, IForecastSource
            => services.AddScoped<IForecastSource, TImplementation>();

        public static IServiceCollection AddForecastSourceWithOptions<TImplementation, TOptions>(this IServiceCollection services, TOptions options)
            where TImplementation : class, IForecastSource
            where TOptions : ApiSourceOptions
        {
            services.AddSingleton<IOptions<TOptions>>(Options.Create(options));
            services.AddScoped<IForecastSource, TImplementation>();

            return services;
        }

        public static IServiceCollection AddGeocoding(
            this IServiceCollection services,
            SourceOptions geocodingOptions,
            SourceOptions countryCodeResolverOptions)
        {
            services.AddHttpClient<IGeocodingService, GeocodingService>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(geocodingOptions.TimeoutSeconds);
                client.BaseAddress = new Uri(geocodingOptions.BaseUrl);
            });

            services.AddHttpClient<ICountryCodeResolver, CountryCodeResolver>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(countryCodeResolverOptions.TimeoutSeconds);
                client.BaseAddress = new Uri(countryCodeResolverOptions.BaseUrl);
            });

            return services;
        }
    }
}
