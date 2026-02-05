using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using WeatherForecastService.Models.Options;
using WeatherForecastService.Services;
using WeatherForecastService.Services.Abstractions;

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

        public static IServiceCollection AddNamedForecastSourceWithOptions<TImplementation, TOptions>(this IServiceCollection services, TOptions options, string name)
            where TImplementation : class, IForecastSource
            where TOptions : ApiSourceOptions
        {
            services.AddSingleton(Options.Create(options));
            services.AddHttpClient(name, client =>
            {
                client.BaseAddress = new Uri(options.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            })
            .AddPolicyHandler(GetRetryPolicy())
            .AddPolicyHandler(GetCircuitBreakerPolicy());

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
            })
            .AddPolicyHandler(GetRetryPolicy())
            .AddPolicyHandler(GetCircuitBreakerPolicy());

            services.AddHttpClient<ICountryCodeResolver, CountryCodeResolver>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(countryCodeResolverOptions.TimeoutSeconds);
                client.BaseAddress = new Uri(countryCodeResolverOptions.BaseUrl);
            })
            .AddPolicyHandler(GetRetryPolicy())
            .AddPolicyHandler(GetCircuitBreakerPolicy());

            return services;
        }

        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
            => HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                );

        private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
            => HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 2,
                    durationOfBreak: TimeSpan.FromSeconds(20)
                );
    }
}
