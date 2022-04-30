using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Text.Json;

var builder = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddUserSecrets<Program>()
    .AddJsonFile("appsettings.json", optional: true)
    .AddEnvironmentVariables();

var configuration = builder.Build();

var uri = configuration.GetValue<string>("uri");
var frequency = configuration.GetValue<int>("frequency");
var location = configuration.GetValue<string>("location");
var connectionstring = configuration.GetValue<string>("connectionstring");

Console.WriteLine($"Starting availability monitoring of {uri} every {frequency} seconds...");

var client = new HttpClient();
var telemetryClient = new TelemetryClient(new TelemetryConfiguration(connectionstring));

while (true)
{
    var start = DateTimeOffset.UtcNow;
    try
    {
        var health = await client.GetFromJsonAsync<HealthEndpointData>(uri);
        var end = DateTimeOffset.UtcNow;
        var metrics = new Dictionary<string, double>();
        var properties = new Dictionary<string, string>();

        if (health?.Components != null)
        {
            foreach (var component in health.Components)
            {
                properties.Add(component.Key, component.Value.Status);
                foreach (var item in component.Value.Details)
                {
                    if (item.Value.ValueKind == JsonValueKind.Number)
                    {
                        metrics.Add($"{component.Key}_{item.Key}", item.Value.GetDouble());
                    }
                    else
                    {
                        properties.Add($"{component.Key}_{item.Key}", item.Value.GetString() ?? string.Empty);
                    }
                }
            }
        }

        telemetryClient.TrackAvailability(
            name: "health",
            timeStamp: start,
            duration: end - start,
            runLocation: location,
            success: string.Equals(health?.Status, "UP"),
            properties: properties, metrics: metrics);
    }
    catch (Exception ex)
    {
        var end = DateTimeOffset.UtcNow;
        telemetryClient.TrackAvailability(
            name: "unhealthy",
            timeStamp: start,
            duration: end - start,
            runLocation: location,
            success: false,
            message: ex.Message);
    }

    telemetryClient.Flush();
    await Task.Delay(TimeSpan.FromSeconds(frequency));
}