namespace CustomerFeedback.Api.Analysis;

public interface IFeedbackAnalysisService
{
    Task<FeedbackAnalysisResult> AnalyzeCommentAsync(
        string comment,
        CancellationToken cancellationToken = default);
}