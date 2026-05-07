namespace Meal.Ai.Models;

public sealed class AiOptions
{
    public string Provider { get; set; } = "LMStudio";
    public string BaseUrl { get; set; } = "http://localhost:1234";
    public string Model { get; set; } = "local-model-name";
    public int TimeoutSeconds { get; set; } = 30;
    public int RetryCount { get; set; } = 1;
}
