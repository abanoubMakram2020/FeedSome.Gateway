// Program.cs
using FeedSome.Gateway.Middleware;
using FeedSome.Gateway.Services;
using FeedSome.Gateway.Transforms;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Add logging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});

// Add our custom services
builder.Services.AddSingleton<ICountryRoutingService, CountryRoutingService>();
builder.Services.AddSingleton<CountryBasedTransform>();

// Configure YARP with config
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(builderContext =>
    {
        builderContext.AddRequestTransform(async transformContext =>
        {
            Console.WriteLine($"Request to: {transformContext.Path}");
        });
    });

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Add our custom middleware
app.UseMiddleware<LoggingMiddleware>();

// Use CORS
app.UseCors();

// Map controllers
app.MapControllers();

// Configure YARP routes with custom routing logic
app.MapReverseProxy();

// Add a catch-all route for proxying
app.MapFallback(async context =>
{
    // Skip if it's already been handled by controllers
    if (context.Response.HasStarted)
        return;

    // For all other requests, proxy them
    await context.Response.WriteAsync("Request will be proxied based on country detection");
});

app.Run();
