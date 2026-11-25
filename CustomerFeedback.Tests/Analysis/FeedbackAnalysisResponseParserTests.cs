using CustomerFeedback.Api.Analysis;

namespace CustomerFeedback.Tests.Analysis;

public class FeedbackAnalysisResponseParserTests
{
    [Fact]
    public void Parse_ValidJson_ReturnsResult()
    {
        var json = """
            {"sentiment":"Positive","category":"Delivery","summary":"Fast shipping praised."}
            """;

        var result = FeedbackAnalysisResponseParser.Parse(json);

        Assert.Equal("Positive", result.Sentiment);
        Assert.Equal("Delivery", result.Category);
        Assert.Equal("Fast shipping praised.", result.Summary);
    }

    [Fact]
    public void Parse_FencedJson_ReturnsResult()
    {
        var json = """
            ```json
            {"sentiment":"Negative","category":"Customer Service","summary":"Support was unhelpful."}
            ```
            """;

        var result = FeedbackAnalysisResponseParser.Parse(json);

        Assert.Equal("Negative", result.Sentiment);
        Assert.Equal("Customer Service", result.Category);
    }

    [Fact]
    public void Parse_UnknownCategory_DefaultsToOther()
    {
        var json = """{"sentiment":"Neutral","category":"Billing","summary":"Some billing note here."}""";

        var result = FeedbackAnalysisResponseParser.Parse(json);

        Assert.Equal("Other", result.Category);
    }

    [Fact]
    public void Parse_InvalidJson_ThrowsInvalidResponse()
    {
        Assert.Throws<FeedbackAnalysisInvalidResponseException>(
            () => FeedbackAnalysisResponseParser.Parse("not-json"));
    }
}