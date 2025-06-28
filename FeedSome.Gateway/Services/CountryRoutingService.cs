using System.Text.Json;
using FeedSome.Gateway.Models;
using Microsoft.Extensions.Logging;

namespace FeedSome.Gateway.Services;

public interface ICountryRoutingService
{
    string GetTargetUrl(string countryCode);
    string DetectCountryCode(HttpContext context);
    RoutingConfig GetRoutingConfig();
}

public class CountryRoutingService : ICountryRoutingService
{
    private readonly ILogger<CountryRoutingService> _logger;
    private readonly RoutingConfig _routingConfig;
    private readonly string _configPath;

    public CountryRoutingService(ILogger<CountryRoutingService> logger, IWebHostEnvironment environment)
    {
        _logger = logger;
        _configPath = Path.Combine(environment.ContentRootPath, "routes.json");
        _routingConfig = LoadRoutingConfig();
    }

    public string GetTargetUrl(string countryCode)
    {
        if (string.IsNullOrEmpty(countryCode))
        {
            _logger.LogWarning("Country code is null or empty, using default route");
            return _routingConfig.DefaultRoute;
        }

        var upperCountryCode = countryCode.ToUpperInvariant();
        
        if (_routingConfig.Routes.TryGetValue(upperCountryCode, out var targetUrl))
        {
            _logger.LogInformation("Routing request for country {CountryCode} to {TargetUrl}", upperCountryCode, targetUrl);
            return targetUrl;
        }

        _logger.LogWarning("No route found for country {CountryCode}, using default route", upperCountryCode);
        return _routingConfig.DefaultRoute;
    }

    public string DetectCountryCode(HttpContext context)
    {
        // First, try to get country code from header
        var countryCode = context.Request.Headers["X-Country-Code"].FirstOrDefault();
        
        if (!string.IsNullOrEmpty(countryCode))
        {
            _logger.LogInformation("Country code detected from header: {CountryCode}", countryCode);
            return countryCode;
        }

        // Try to get from query parameter
        countryCode = context.Request.Query["country"].FirstOrDefault();
        
        if (!string.IsNullOrEmpty(countryCode))
        {
            _logger.LogInformation("Country code detected from query parameter: {CountryCode}", countryCode);
            return countryCode;
        }

        // For IP-based detection, you would typically use a GeoIP service
        // This is a simplified example - in production, you'd use a service like MaxMind
        var clientIp = GetClientIpAddress(context);
        _logger.LogInformation("No country code found in headers/query, client IP: {ClientIp}", clientIp);
        
        // For demo purposes, return a default country code
        // In a real implementation, you would look up the IP in a GeoIP database
        return "US";
    }

    public RoutingConfig GetRoutingConfig()
    {
        return _routingConfig;
    }

    private RoutingConfig LoadRoutingConfig()
    {
        try
        {
            if (!File.Exists(_configPath))
            {
                _logger.LogError("Routes configuration file not found at {ConfigPath}", _configPath);
                return new RoutingConfig
                {
                    Routes = new Dictionary<string, string>(),
                    DefaultRoute = "https://us.example.com"
                };
            }

            var jsonContent = File.ReadAllText(_configPath);
            var config = JsonSerializer.Deserialize<RoutingConfig>(jsonContent);
            
            if (config == null)
            {
                _logger.LogError("Failed to deserialize routing configuration");
                return new RoutingConfig
                {
                    Routes = new Dictionary<string, string>(),
                    DefaultRoute = "https://us.example.com"
                };
            }

            _logger.LogInformation("Loaded routing configuration with {RouteCount} routes", config.Routes.Count);
            return config;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading routing configuration");
            return new RoutingConfig
            {
                Routes = new Dictionary<string, string>(),
                DefaultRoute = "https://us.example.com"
            };
        }
    }

    private string GetClientIpAddress(HttpContext context)
    {
        // Check for forwarded headers first (for when behind a proxy/load balancer)
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }
} 