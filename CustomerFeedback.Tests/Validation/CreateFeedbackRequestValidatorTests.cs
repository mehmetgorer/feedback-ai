using CustomerFeedback.Api.Dtos;
using CustomerFeedback.Api.Validation;

namespace CustomerFeedback.Tests.Validation;

public class CreateFeedbackRequestValidatorTests
{
    private readonly CreateFeedbackRequestValidator _validator = new();

    private static CreateFeedbackRequest ValidRequest() => new()
    {
        CustomerName = "Jane Doe",
        CustomerEmail = "jane@example.com",
        Rating = 5,
        Comment = "Great service overall."
    };

    [Fact]
    public void Validate_ValidRequest_IsValid()
    {
        var result = _validator.Validate(ValidRequest());
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyCustomerName_IsInvalid()
    {
        var request = ValidRequest();
        request.CustomerName = "";

        var result = _validator.Validate(request);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateFeedbackRequest.CustomerName));
    }

    [Fact]
    public void Validate_InvalidEmail_IsInvalid()
    {
        var request = ValidRequest();
        request.CustomerEmail = "not-an-email";

        var result = _validator.Validate(request);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateFeedbackRequest.CustomerEmail));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void Validate_RatingOutOfRange_IsInvalid(int rating)
    {
        var request = ValidRequest();
        request.Rating = rating;

        var result = _validator.Validate(request);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateFeedbackRequest.Rating));
    }

    [Fact]
    public void Validate_ShortComment_IsInvalid()
    {
        var request = ValidRequest();
        request.Comment = "Too short";

        var result = _validator.Validate(request);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateFeedbackRequest.Comment));
    }
}