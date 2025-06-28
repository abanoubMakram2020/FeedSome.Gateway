using FeedSome.Gateway.Services;

namespace FeedSome.Gateway.Transforms;

public class CountryBasedTransform
{
    private readonly ICountryRoutingService _routingService;
    private readonly ILogger<CountryBasedTransform> _logger;

    public CountryBasedTransform(ICountryRoutingService routingService, ILogger<CountryBasedTransform> logger)
    {
        _routingService = routingService;
        _logger = logger;
    }

    public void TransformRequest(HttpContext context)
    {
        try
        {
            var countryCode = _routingService.DetectCountryCode(context);
            var targetUrl = _routingService.GetTargetUrl(countryCode);

            // Parse the target URL to get the destination
            if (Uri.TryCreate(targetUrl, UriKind.Absolute, out var uri))
            {
                // Store the target URL in the context for the proxy to use
                context.Items["TargetUrl"] = targetUrl;
                
                _logger.LogInformation(
                    "Transformed request for country {CountryCode} to {TargetUrl}",
                    countryCode, targetUrl);
            }
            else
            {
                _logger.LogError("Invalid target URL: {TargetUrl}", targetUrl);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying country-based transform");
        }
    }
} 