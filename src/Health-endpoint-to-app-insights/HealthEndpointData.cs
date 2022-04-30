using System.Text.Json;
using System.Text.Json.Serialization;

public class HealthEndpointData
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("components")]
    public Dictionary<string, Component> Components { get; set; } = new();

}

public class Component
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("details")]
    public Dictionary<string, JsonElement> Details { get; set; } = new();
}
