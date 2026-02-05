namespace WeatherForecastService.Exceptions
{
    public class ForecastSourceException(string sourceName, string error) : Exception($"Source name: {sourceName} | Error: {error}")
    {
    }
}
