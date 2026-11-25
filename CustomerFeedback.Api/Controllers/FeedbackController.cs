using CustomerFeedback.Api.Dtos;
using CustomerFeedback.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CustomerFeedback.Api.Controllers;

[ApiController]
[Route("api/feedback")]
public class FeedbackController(IFeedbackService feedbackService, ILogger<FeedbackController> logger) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<FeedbackResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<FeedbackResponse>>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var items = await feedbackService.ListAsync(page, pageSize, cancellationToken);
        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(FeedbackResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FeedbackResponse>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await feedbackService.GetByIdAsync(id, cancellationToken);
        if (item is null)
        {
            return NotFound();
        }

        return Ok(item);
    }

    [HttpPost]
    [ProducesResponseType(typeof(FeedbackResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<FeedbackResponse>> Create(
        [FromBody] CreateFeedbackRequest request,
        CancellationToken cancellationToken = default)
    {
        var created = await feedbackService.CreateAsync(request, cancellationToken);
        logger.LogInformation("Created feedback {FeedbackId}", created.Id);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(FeedbackResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<FeedbackResponse>> Update(
        Guid id,
        [FromBody] UpdateFeedbackRequest request,
        CancellationToken cancellationToken = default)
    {
        var updated = await feedbackService.UpdateAsync(id, request, cancellationToken);
        if (updated is null)
        {
            return NotFound();
        }

        logger.LogInformation("Updated feedback {FeedbackId}", id);
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        var deleted = await feedbackService.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }

        logger.LogInformation("Deleted feedback {FeedbackId}", id);
        return NoContent();
    }
}