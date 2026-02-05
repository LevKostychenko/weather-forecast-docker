namespace WeatherForecastService.Services.Abstractions
{
    public interface IForecastSourceManager
    {
        IEnumerable<IForecastSource> GetForecastSources();
    }
}
