using System.Net;
using System.Net.Http.Json;
using CustomerFeedback.Api.Dtos;
using CustomerFeedback.Tests.Infrastructure;

namespace CustomerFeedback.Tests.Integration;

public class FeedbackApiTests : IClassFixture<FeedbackApiFactory>
{
    private readonly HttpClient _client;

    public FeedbackApiTests(FeedbackApiFactory factory)
    {
        factory.InitializeDatabase();
        _client = factory.CreateClient();
    }

    private static CreateFeedbackRequest SampleRequest(string suffix = "") => new()
    {
        CustomerName = $"Customer {suffix}",
        CustomerEmail = $"customer{suffix}@example.com",
        Rating = 4,
        Comment = $"Feedback comment {suffix} is detailed enough."
    };

    [Fact]
    public async Task Create_Get_Update_Delete_FullCrudFlow_Succeeds()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/feedback", SampleRequest("1"));
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<FeedbackResponse>();
        Assert.NotNull(created);
        Assert.Equal("Customer 1", created.CustomerName);

        var getResponse = await _client.GetAsync($"/api/feedback/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var updateRequest = new UpdateFeedbackRequest
        {
            CustomerName = "Updated Customer",
            CustomerEmail = "updated@example.com",
            Rating = 2,
            Comment = "Updated comment with enough length."
        };

        var updateResponse = await _client.PutAsJsonAsync($"/api/feedback/{created.Id}", updateRequest);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var updated = await updateResponse.Content.ReadFromJsonAsync<FeedbackResponse>();
        Assert.NotNull(updated);
        Assert.Equal("Updated Customer", updated.CustomerName);
        Assert.NotNull(updated.UpdatedAtUtc);

        var deleteResponse = await _client.DeleteAsync($"/api/feedback/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var missingResponse = await _client.GetAsync($"/api/feedback/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, missingResponse.StatusCode);
    }

    [Fact]
    public async Task Create_InvalidRequest_ReturnsBadRequest()
    {
        var invalid = new CreateFeedbackRequest
        {
            CustomerName = "",
            CustomerEmail = "bad-email",
            Rating = 10,
            Comment = "short"
        };

        var response = await _client.PostAsJsonAsync("/api/feedback", invalid);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetById_UnknownId_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/feedback/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_UnknownId_ReturnsNotFound()
    {
        var response = await _client.PutAsJsonAsync(
            $"/api/feedback/{Guid.NewGuid()}",
            new UpdateFeedbackRequest
            {
                CustomerName = "Name",
                CustomerEmail = "name@example.com",
                Rating = 3,
                Comment = "Valid update comment."
            });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_InvalidRequest_ReturnsBadRequest()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/feedback", SampleRequest("update"));
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<FeedbackResponse>();
        Assert.NotNull(created);

        var invalid = new UpdateFeedbackRequest
        {
            CustomerName = "",
            CustomerEmail = "bad-email",
            Rating = 10,
            Comment = "short"
        };

        var response = await _client.PutAsJsonAsync($"/api/feedback/{created.Id}", invalid);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Delete_UnknownId_ReturnsNotFound()
    {
        var response = await _client.DeleteAsync($"/api/feedback/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task List_WithPagination_ReturnsRequestedPageSize()
    {
        for (var i = 0; i < 3; i++)
        {
            await _client.PostAsJsonAsync("/api/feedback", SampleRequest($"page-{i}"));
        }

        var response = await _client.GetAsync("/api/feedback?page=1&pageSize=2");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var items = await response.Content.ReadFromJsonAsync<List<FeedbackResponse>>();
        Assert.NotNull(items);
        Assert.Equal(2, items.Count);
    }
}