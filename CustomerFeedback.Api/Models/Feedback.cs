namespace CustomerFeedback.Api.Models;

public class Feedback
{
    public Guid Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public string? AnalysisSentiment { get; set; }
    public string? AnalysisCategory { get; set; }
    public string? AnalysisSummary { get; set; }
    public DateTime? AnalyzedAtUtc { get; set; }
}