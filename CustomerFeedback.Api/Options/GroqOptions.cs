namespace CustomerFeedback.Api.Options;

public sealed class GroqOptions
{
    public const string SectionName = "Groq";

    public string? ApiKey { get; set; }

    public string Model { get; set; } = "openai/gpt-oss-20b";

    public int TimeoutSeconds { get; set; } = 30;

    public string BaseUrl { get; set; } = "https://api.groq.com/openai/v1";
}