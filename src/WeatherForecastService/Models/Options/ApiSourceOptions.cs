namespace WeatherForecastService.Models.Options
{
    public record ApiSourceOptions : SourceOptions
    {
        public string ApiKey { get; set; } = string.Empty;
    }
}
