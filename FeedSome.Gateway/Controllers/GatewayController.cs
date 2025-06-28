using FeedSome.Gateway.Services;
using Microsoft.AspNetCore.Mvc;

namespace FeedSome.Gateway.Controllers;

[ApiController]
[Route("[controller]")]
public class GatewayController : ControllerBase
{
    private readonly ICountryRoutingService _routingService;
    private readonly ILogger<GatewayController> _logger;

    public GatewayController(ICountryRoutingService routingService, ILogger<GatewayController> logger)
    {
        _routingService = routingService;
        _logger = logger;
    }

    [HttpGet("/")]
    public IActionResult GetStatus()
    {
        _logger.LogInformation("Gateway status endpoint accessed");
        return Ok(new
        {
            message = "Gateway is running",
            timestamp = DateTime.UtcNow,
            version = "1.0.0",
            framework = ".NET 8.0"
        });
    }

    [HttpGet("status")]
    public IActionResult GetDetailedStatus()
    {
        var config = _routingService.GetRoutingConfig();
        return Ok(new
        {
            status = "Running",
            timestamp = DateTime.UtcNow,
            version = "1.0.0",
            framework = ".NET 8.0",
            routing = new
            {
                availableCountries = config.Routes.Keys.ToArray(),
                defaultRoute = config.DefaultRoute,
                totalRoutes = config.Routes.Count
            }
        });
    }

    [HttpGet("routes")]
    public IActionResult GetRoutes()
    {
        var config = _routingService.GetRoutingConfig();
        return Ok(config.Routes);
    }

    [HttpGet("detect")]
    public IActionResult DetectCountry()
    {
        var countryCode = _routingService.DetectCountryCode(HttpContext);
        var targetUrl = _routingService.GetTargetUrl(countryCode);
        
        return Ok(new
        {
            detectedCountry = countryCode,
            targetUrl = targetUrl,
            timestamp = DateTime.UtcNow
        });
    }
} 