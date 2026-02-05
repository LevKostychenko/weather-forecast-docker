using WeatherForecastService.Services.Abstractions;

namespace WeatherForecastService.Services
{
    internal class ForecastSourceManager(IServiceProvider provider) : IForecastSourceManager
    {
        public IEnumerable<IForecastSource> GetForecastSources()
            => provider.GetServices<IForecastSource>();
    }
}
