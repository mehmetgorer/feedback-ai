namespace CustomerFeedback.Api.Dtos;

public class FeedbackAnalysisDto
{
    public string Sentiment { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public DateTime AnalyzedAtUtc { get; set; }
}