namespace CustomerFeedback.Api.Analysis;

public sealed class FeedbackAnalysisResult
{
    public required string Sentiment { get; init; }
    public required string Category { get; init; }
    public required string Summary { get; init; }
}