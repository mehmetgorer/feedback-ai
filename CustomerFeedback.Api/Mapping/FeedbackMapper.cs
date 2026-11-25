using CustomerFeedback.Api.Dtos;
using CustomerFeedback.Api.Models;

namespace CustomerFeedback.Api.Mapping;

public static class FeedbackMapper
{
    public static FeedbackResponse ToResponse(Feedback entity)
    {
        FeedbackAnalysisDto? analysis = null;
        if (entity.AnalyzedAtUtc is not null
            && entity.AnalysisSentiment is not null
            && entity.AnalysisCategory is not null
            && entity.AnalysisSummary is not null)
        {
            analysis = new FeedbackAnalysisDto
            {
                Sentiment = entity.AnalysisSentiment,
                Category = entity.AnalysisCategory,
                Summary = entity.AnalysisSummary,
                AnalyzedAtUtc = entity.AnalyzedAtUtc.Value,
            };
        }

        return new FeedbackResponse
        {
            Id = entity.Id,
            CustomerName = entity.CustomerName,
            CustomerEmail = entity.CustomerEmail,
            Rating = entity.Rating,
            Comment = entity.Comment,
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc,
            Analysis = analysis,
        };
    }

    public static void ClearAnalysis(Feedback entity)
    {
        entity.AnalysisSentiment = null;
        entity.AnalysisCategory = null;
        entity.AnalysisSummary = null;
        entity.AnalyzedAtUtc = null;
    }
}