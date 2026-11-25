using CustomerFeedback.Api.Analysis;

namespace CustomerFeedback.Tests.Infrastructure;

public sealed class FakeFeedbackAnalysisService : IFeedbackAnalysisService
{
    public Func<string, FeedbackAnalysisResult>? Handler { get; set; }

    public FeedbackAnalysisException? ExceptionToThrow { get; set; }

    public string? LastComment { get; private set; }

    public Task<FeedbackAnalysisResult> AnalyzeCommentAsync(
        string comment,
        CancellationToken cancellationToken = default)
    {
        LastComment = comment;

        if (ExceptionToThrow is not null)
        {
            throw ExceptionToThrow;
        }

        var result = Handler?.Invoke(comment) ?? new FeedbackAnalysisResult
        {
            Sentiment = "Positive",
            Category = "Product",
            Summary = "Mock analysis summary for testing.",
        };

        return Task.FromResult(result);
    }
}