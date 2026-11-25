using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using CustomerFeedback.Api.Options;
using Microsoft.Extensions.Options;

namespace CustomerFeedback.Api.Analysis;

public sealed class GroqFeedbackAnalysisService(
    HttpClient httpClient,
    IOptions<GroqOptions> options,
    ILogger<GroqFeedbackAnalysisService> logger) : IFeedbackAnalysisService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private const string SystemPrompt =
        """
        You analyze customer feedback comments. Return only JSON with these exact keys:
        - sentiment: one of Positive, Neutral, Negative
        - category: one of Product, Delivery, Customer Service, Other
        - summary: one or two short sentences (max 200 characters) describing the feedback
        
        Do not include markdown or extra text outside the JSON object.
        """;

    public async Task<FeedbackAnalysisResult> AnalyzeCommentAsync(
        string comment,
        CancellationToken cancellationToken = default)
    {
        var settings = options.Value;
        if (string.IsNullOrWhiteSpace(settings.ApiKey))
        {
            throw new FeedbackAnalysisNotConfiguredException();
        }

        if (string.IsNullOrWhiteSpace(comment))
        {
            throw new FeedbackAnalysisInvalidResponseException("Comment is empty.");
        }

        var requestBody = new
        {
            model = settings.Model,
            temperature = 0.2,
            response_format = new { type = "json_object" },
            messages = new object[]
            {
                new { role = "system", content = SystemPrompt },
                new { role = "user", content = comment },
            },
        };

        var json = JsonSerializer.Serialize(requestBody, JsonOptions);
        using var request = new HttpRequestMessage(HttpMethod.Post, "chat/completions")
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
        };

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ApiKey);

        HttpResponseMessage response;
        try
        {
            response = await httpClient.SendAsync(request, cancellationToken);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(ex, "Groq request timed out");
            throw new FeedbackAnalysisTimeoutException(ex);
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "Groq request failed");
            throw new FeedbackAnalysisProviderException("Could not reach the AI provider.", ex);
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning(
                "Groq returned {StatusCode}: {Body}",
                (int)response.StatusCode,
                Truncate(body, 300));

            var message = response.StatusCode switch
            {
                HttpStatusCode.TooManyRequests => "The AI provider rate limit was exceeded.",
                HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden =>
                    "The AI provider rejected the API key.",
                _ => "The AI provider returned an error.",
            };

            throw new FeedbackAnalysisProviderException(message);
        }

        string content;
        try
        {
            using var document = JsonDocument.Parse(body);
            content = document.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? string.Empty;
        }
        catch (Exception ex)
        {
            throw new FeedbackAnalysisInvalidResponseException("Could not read AI response content.", ex);
        }

        return FeedbackAnalysisResponseParser.Parse(content);
    }

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength];
}