namespace WeatherForecastService.Models.Options
{
    public record SourceOptions
    {
        public string BaseUrl { get; set; } = string.Empty;
        public int TimeoutSeconds { get; set; } = 60;
    }
}
