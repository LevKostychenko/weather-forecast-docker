namespace WeatherForecastService.Exceptions
{
    public class GeocodingException(string error) : Exception(error)
    {
    }
}
