namespace WeatherForecastService.Models
{
    public record AggregatedWeatherForecast
    {
        public IDictionary<string, WeatherForecast?> Forecasts { get; set; } = new Dictionary<string, WeatherForecast?>();
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public DateTime TimeStamp { get; set; }
    }
}
