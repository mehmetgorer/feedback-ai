using System.Net;
using System.Net.Http.Json;
using CustomerFeedback.Api.Analysis;
using CustomerFeedback.Api.Dtos;
using CustomerFeedback.Tests.Infrastructure;

namespace CustomerFeedback.Tests.Integration;

public class FeedbackAnalysisApiTests : IClassFixture<FeedbackApiFactory>
{
    private readonly HttpClient _client;
    private readonly FeedbackApiFactory _factory;

    public FeedbackAnalysisApiTests(FeedbackApiFactory factory)
    {
        factory.InitializeDatabase();
        _factory = factory;
        _client = factory.CreateClient();
        _factory.AnalysisService.ExceptionToThrow = null;
        _factory.AnalysisService.Handler = null;
    }

    private static CreateFeedbackRequest SampleRequest(string suffix = "") => new()
    {
        CustomerName = $"Customer {suffix}",
        CustomerEmail = $"customer{suffix}@example.com",
        Rating = 4,
        Comment = $"Delivery was quick and the product works well ({suffix}).",
    };

    [Fact]
    public async Task Analyze_ExistingFeedback_StoresAndReturnsAnalysis()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/feedback", SampleRequest("analyze"));
        var created = await createResponse.Content.ReadFromJsonAsync<FeedbackResponse>();
        Assert.NotNull(created);

        var analyzeResponse = await _client.PostAsync($"/api/feedback/{created.Id}/analyze", null);
        Assert.Equal(HttpStatusCode.OK, analyzeResponse.StatusCode);

        var analyzed = await analyzeResponse.Content.ReadFromJsonAsync<FeedbackResponse>();
        Assert.NotNull(analyzed);
        Assert.NotNull(analyzed.Analysis);
        Assert.Equal("Positive", analyzed.Analysis.Sentiment);
        Assert.Equal("Product", analyzed.Analysis.Category);
        Assert.False(string.IsNullOrWhiteSpace(analyzed.Analysis.Summary));
        Assert.Equal(created.Comment, _factory.AnalysisService.LastComment);

        var getResponse = await _client.GetAsync($"/api/feedback/{created.Id}");
        var loaded = await getResponse.Content.ReadFromJsonAsync<FeedbackResponse>();
        Assert.NotNull(loaded?.Analysis);
    }

    [Fact]
    public async Task Analyze_UnknownId_ReturnsNotFound()
    {
        var response = await _client.PostAsync($"/api/feedback/{Guid.NewGuid()}/analyze", null);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Analyze_ProviderFailure_Returns502()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/feedback", SampleRequest("fail"));
        var created = await createResponse.Content.ReadFromJsonAsync<FeedbackResponse>();
        Assert.NotNull(created);

        _factory.AnalysisService.ExceptionToThrow =
            new FeedbackAnalysisProviderException("Provider down");

        var response = await _client.PostAsync($"/api/feedback/{created.Id}/analyze", null);
        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
    }

    [Fact]
    public async Task Analyze_InvalidAiResponse_Returns502()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/feedback", SampleRequest("invalid"));
        var created = await createResponse.Content.ReadFromJsonAsync<FeedbackResponse>();
        Assert.NotNull(created);

        _factory.AnalysisService.ExceptionToThrow =
            new FeedbackAnalysisInvalidResponseException("Bad JSON from model");

        var response = await _client.PostAsync($"/api/feedback/{created.Id}/analyze", null);
        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
    }

    [Fact]
    public async Task Analyze_NotConfigured_Returns503()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/feedback", SampleRequest("nocfg"));
        var created = await createResponse.Content.ReadFromJsonAsync<FeedbackResponse>();
        Assert.NotNull(created);

        _factory.AnalysisService.ExceptionToThrow = new FeedbackAnalysisNotConfiguredException();

        var response = await _client.PostAsync($"/api/feedback/{created.Id}/analyze", null);
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task Update_Comment_ClearsStoredAnalysis()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/feedback", SampleRequest("clear"));
        var created = await createResponse.Content.ReadFromJsonAsync<FeedbackResponse>();
        Assert.NotNull(created);

        await _client.PostAsync($"/api/feedback/{created.Id}/analyze", null);

        var updateRequest = new UpdateFeedbackRequest
        {
            CustomerName = created.CustomerName,
            CustomerEmail = created.CustomerEmail,
            Rating = created.Rating,
            Comment = "Updated comment with enough length for validation.",
        };

        var updateResponse = await _client.PutAsJsonAsync($"/api/feedback/{created.Id}", updateRequest);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var updated = await updateResponse.Content.ReadFromJsonAsync<FeedbackResponse>();
        Assert.NotNull(updated);
        Assert.Null(updated.Analysis);
    }
}