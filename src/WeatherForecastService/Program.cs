using Microsoft.Extensions.Configuration;
using WeatherForecastService.Extensions;
using WeatherForecastService.Models.Options;
using WeatherForecastService.Services;
using WeatherForecastService.Services.Abstractions;
using WeatherForecastService.Services.ForecastSources;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddCache();

var geocodingOptions = builder.Configuration.GetSection("Geocoding").Get<SourceOptions>()
    ?? throw new NullReferenceException("Geocoding options are not provided");
var countryCodeResolverOptions = builder.Configuration.GetSection("CountryCodeResolving").Get<SourceOptions>()
    ?? throw new NullReferenceException("CountryCodeResolving options are not provided");
var windySourceOptions = builder.Configuration.GetSection("ForecastSources:Windy").Get<WindySourceOptions>()
    ?? throw new NullReferenceException("Windy source options are not provided");
var meteoblueSourceOptions = builder.Configuration.GetSection("ForecastSources:Meteoblue").Get<MeteoblueSourceOptions>()
    ?? throw new NullReferenceException("Meteoblue source options are not provided");

builder.Services
    .AddGeocoding(geocodingOptions, countryCodeResolverOptions)
    .AddScoped<IForecastSourceManager, ForecastSourceManager>()
    .AddScoped<IForecastService, ForecastService>()
    .AddForecastSource<OpenMeteoSource>()
    .AddForecastSourceWithOptions<WindySource, WindySourceOptions>(windySourceOptions)
    .AddForecastSourceWithOptions<MeteoblueSource, MeteoblueSourceOptions>(meteoblueSourceOptions);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
