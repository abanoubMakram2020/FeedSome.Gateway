# FeedSome Gateway

A .NET 8 YARP (Yet Another Reverse Proxy) Gateway that routes requests based on country codes.

## Features

- **Country-based Routing**: Routes requests to different backend URLs based on country codes
- **Multiple Detection Methods**: Supports country detection via headers (`X-Country-Code`) and query parameters
- **JSON Configuration**: Easy-to-modify routing configuration via `routes.json`
- **Comprehensive Logging**: Detailed request/response logging with performance metrics
- **Error Handling**: Robust error handling with meaningful error responses
- **Health Endpoints**: Built-in status and monitoring endpoints

## Quick Start

### Prerequisites

- .NET 8.0 SDK or later
- Visual Studio 2022, VS Code, or any .NET-compatible IDE

### Running the Gateway

1. **Clone and navigate to the project:**
   ```bash
   cd FeedSome.Gateway
   ```

2. **Run the application:**
   ```bash
   dotnet run
   ```

3. **Access the gateway:**
   - Gateway Status: `http://localhost:5000/`
   - Detailed Status: `http://localhost:5000/gateway/status`
   - Available Routes: `http://localhost:5000/gateway/routes`
   - Country Detection: `http://localhost:5000/gateway/detect`

## Configuration

### Routes Configuration

Edit `routes.json` to configure country-based routing:

```json
{
  "routes": {
    "EG": "https://eg.example.com",
    "US": "https://us.example.com",
    "GB": "https://gb.example.com",
    "DE": "https://de.example.com"
  },
  "defaultRoute": "https://us.example.com"
}
```

### Country Detection

The gateway supports multiple ways to detect the country:

1. **Header-based**: Set `X-Country-Code` header
   ```bash
   curl -H "X-Country-Code: EG" http://localhost:5000/api/data
   ```

2. **Query Parameter**: Use `?country=EG`
   ```bash
   curl "http://localhost:5000/api/data?country=EG"
   ```

3. **IP-based**: Automatically detected (currently defaults to US)

## API Endpoints

### Gateway Status
- `GET /` - Basic gateway status
- `GET /gateway/status` - Detailed status with routing information
- `GET /gateway/routes` - List all configured routes
- `GET /gateway/detect` - Test country detection

### Proxy Endpoints
All other requests are automatically proxied based on country detection.

## Example Usage

### Testing with Different Countries

```bash
# Egyptian traffic
curl -H "X-Country-Code: EG" http://localhost:5000/api/users

# US traffic
curl -H "X-Country-Code: US" http://localhost:5000/api/users

# German traffic
curl -H "X-Country-Code: DE" http://localhost:5000/api/users
```

### Checking Gateway Status

```bash
# Basic status
curl http://localhost:5000/

# Detailed status
curl http://localhost:5000/gateway/status

# Available routes
curl http://localhost:5000/gateway/routes
```

## Project Structure

```
FeedSome.Gateway/
├── Controllers/
│   └── GatewayController.cs          # Status and monitoring endpoints
├── Middleware/
│   └── LoggingMiddleware.cs          # Request logging and error handling
├── Models/
│   └── RoutingConfig.cs              # Configuration model
├── Services/
│   └── CountryRoutingService.cs      # Country detection and routing logic
├── Transforms/
│   └── CountryBasedTransform.cs      # YARP transform for dynamic routing
├── routes.json                       # Country-to-backend mapping
├── appsettings.json                  # Application configuration
└── Program.cs                        # Application entry point
```

## Logging

The gateway provides comprehensive logging:

- **Request Logging**: All incoming requests with IP, method, path, and user agent
- **Performance Metrics**: Request duration and status codes
- **Routing Information**: Country detection and target URL selection
- **Error Handling**: Detailed error logs with stack traces

Logs are written to console and can be configured in `appsettings.json`.

## Customization

### Adding New Countries

1. Edit `routes.json`
2. Add new country code and target URL
3. Restart the application

### Implementing IP-based Geo-location

To implement real IP-based country detection:

1. Add a GeoIP service (e.g., MaxMind GeoIP2)
2. Modify `CountryRoutingService.DetectCountryCode()` method
3. Add the GeoIP database or service integration

### Custom Transforms

Extend the `CountryBasedTransform` class to add custom request/response transformations.

## Troubleshooting

### Common Issues

1. **Routes not loading**: Check that `routes.json` exists and is valid JSON
2. **Country detection not working**: Verify headers or query parameters are set correctly
3. **Proxy errors**: Check that target URLs are accessible and valid

### Debug Mode

Run with debug logging:
```bash
dotnet run --environment Development
```

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## License

This project is licensed under the MIT License. 