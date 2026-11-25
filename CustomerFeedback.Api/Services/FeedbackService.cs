using CustomerFeedback.Api.Data;
using CustomerFeedback.Api.Dtos;
using CustomerFeedback.Api.Mapping;
using CustomerFeedback.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CustomerFeedback.Api.Services;

public class FeedbackService(AppDbContext dbContext) : IFeedbackService
{
    public async Task<IReadOnlyList<FeedbackResponse>> ListAsync(
        int page,
        int pageSize,
        int? rating = null,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Feedbacks.AsNoTracking();

        if (rating is not null)
        {
            query = query.Where(f => f.Rating == rating.Value);
        }

        var items = await query
            .OrderByDescending(f => f.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return items.Select(FeedbackMapper.ToResponse).ToList();
    }

    public async Task<FeedbackResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.Feedbacks
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);

        return entity is null ? null : FeedbackMapper.ToResponse(entity);
    }

    public async Task<FeedbackResponse> CreateAsync(
        CreateFeedbackRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = new Feedback
        {
            Id = Guid.NewGuid(),
            CustomerName = request.CustomerName,
            CustomerEmail = request.CustomerEmail,
            Rating = request.Rating,
            Comment = request.Comment,
            CreatedAtUtc = DateTime.UtcNow
        };

        dbContext.Feedbacks.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return FeedbackMapper.ToResponse(entity);
    }

    public async Task<FeedbackResponse?> UpdateAsync(
        Guid id,
        UpdateFeedbackRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.Feedbacks.FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        entity.CustomerName = request.CustomerName;
        entity.CustomerEmail = request.CustomerEmail;
        entity.Rating = request.Rating;
        entity.Comment = request.Comment;
        entity.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return FeedbackMapper.ToResponse(entity);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.Feedbacks.FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
        if (entity is null)
        {
            return false;
        }

        dbContext.Feedbacks.Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}