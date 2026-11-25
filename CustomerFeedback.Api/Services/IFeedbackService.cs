using CustomerFeedback.Api.Dtos;

namespace CustomerFeedback.Api.Services;

public interface IFeedbackService
{
    Task<IReadOnlyList<FeedbackResponse>> ListAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<FeedbackResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<FeedbackResponse> CreateAsync(CreateFeedbackRequest request, CancellationToken cancellationToken = default);
    Task<FeedbackResponse?> UpdateAsync(Guid id, UpdateFeedbackRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}