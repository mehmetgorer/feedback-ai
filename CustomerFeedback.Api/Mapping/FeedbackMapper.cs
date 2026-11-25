using CustomerFeedback.Api.Dtos;
using CustomerFeedback.Api.Models;

namespace CustomerFeedback.Api.Mapping;

public static class FeedbackMapper
{
    public static FeedbackResponse ToResponse(Feedback entity) =>
        new()
        {
            Id = entity.Id,
            CustomerName = entity.CustomerName,
            CustomerEmail = entity.CustomerEmail,
            Rating = entity.Rating,
            Comment = entity.Comment,
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc
        };
}