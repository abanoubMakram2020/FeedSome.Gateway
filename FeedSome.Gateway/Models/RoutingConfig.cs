namespace FeedSome.Gateway.Models;

public class RoutingConfig
{
    public Dictionary<string, string> Routes { get; set; } = new();
    public string DefaultRoute { get; set; } = string.Empty;
} 