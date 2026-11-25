namespace CustomerFeedback.Api.Analysis;

public static class FeedbackAnalysisConstants
{
    public static readonly IReadOnlySet<string> Sentiments = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "Positive",
        "Neutral",
        "Negative",
    };

    public static readonly IReadOnlySet<string> Categories = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "Product",
        "Delivery",
        "Customer Service",
        "Other",
    };
}