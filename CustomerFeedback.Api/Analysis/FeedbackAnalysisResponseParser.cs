using System.Text.Json;

namespace CustomerFeedback.Api.Analysis;

public static class FeedbackAnalysisResponseParser
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public static FeedbackAnalysisResult Parse(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new FeedbackAnalysisInvalidResponseException("AI response was empty.");
        }

        var trimmed = content.Trim();
        if (trimmed.StartsWith("```", StringComparison.Ordinal))
        {
            trimmed = StripMarkdownFence(trimmed);
        }

        try
        {
            using var document = JsonDocument.Parse(trimmed);
            var root = document.RootElement;

            var sentiment = GetStringProperty(root, "sentiment");
            var category = GetStringProperty(root, "category");
            var summary = GetStringProperty(root, "summary");

            sentiment = NormalizeSentiment(sentiment);
            category = NormalizeCategory(category);

            if (string.IsNullOrWhiteSpace(summary))
            {
                throw new FeedbackAnalysisInvalidResponseException("AI response did not include a summary.");
            }

            summary = summary.Trim();
            if (summary.Length > 500)
            {
                summary = summary[..500];
            }

            return new FeedbackAnalysisResult
            {
                Sentiment = sentiment,
                Category = category,
                Summary = summary,
            };
        }
        catch (JsonException ex)
        {
            throw new FeedbackAnalysisInvalidResponseException("AI response was not valid JSON.", ex);
        }
    }

    private static string GetStringProperty(JsonElement root, string name)
    {
        if (!root.TryGetProperty(name, out var property) || property.ValueKind != JsonValueKind.String)
        {
            throw new FeedbackAnalysisInvalidResponseException($"AI response is missing '{name}'.");
        }

        return property.GetString() ?? string.Empty;
    }

    private static string NormalizeSentiment(string value)
    {
        if (FeedbackAnalysisConstants.Sentiments.Contains(value))
        {
            return FeedbackAnalysisConstants.Sentiments
                .First(s => s.Equals(value, StringComparison.OrdinalIgnoreCase));
        }

        return "Neutral";
    }

    private static string NormalizeCategory(string value)
    {
        if (FeedbackAnalysisConstants.Categories.Contains(value))
        {
            return FeedbackAnalysisConstants.Categories
                .First(c => c.Equals(value, StringComparison.OrdinalIgnoreCase));
        }

        return "Other";
    }

    private static string StripMarkdownFence(string content)
    {
        var lines = content.Split('\n');
        if (lines.Length < 2)
        {
            return content;
        }

        var inner = lines.Skip(1).TakeWhile(l => !l.TrimStart().StartsWith("```", StringComparison.Ordinal)).ToArray();
        return string.Join('\n', inner).Trim();
    }
}