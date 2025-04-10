using System.Net;
using System.Text;
using System.Text.Json;
using IssueManager.Core.Models;
using IssueManager.Core.Models.Enums;
using IssueManager.Core.Services;
using IssueManager.Core.Services.Helpers;
using FluentAssertions;
using Moq;
using Moq.Protected;

namespace IssueManager.Core.Tests.Services;

public class GitLabIssueServiceTests
{
    private GitLabIssueService CreateService(HttpResponseMessage response)
    {
        var handlerMock = new Mock<HttpMessageHandler>();

        handlerMock
           .Protected()
           .Setup<Task<HttpResponseMessage>>(
               "SendAsync",
               ItExpr.IsAny<HttpRequestMessage>(),
               ItExpr.IsAny<CancellationToken>())
           .ReturnsAsync(response);

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("https://gitlab.com")
        };

        return new GitLabIssueService(httpClient, "test-token");
    }

    [Fact]
    public async Task CreateIssueAsync_WhenSuccess_ReturnsCorrectIssueResponse()
    {
        var gitlabResponse = new
        {
            iid = 123,
            title = "Test title",
            state = "opened",
            web_url = "https://gitlab.com/example/repo/-/issues/123"
        };

        var httpResponse = new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent(JsonSerializer.Serialize(gitlabResponse), Encoding.UTF8, "application/json")
        };

        var service = CreateService(httpResponse);
        var request = new IssueRequest { Title = "Test title", Description = "Test description" };

        var result = await service.CreateIssueAsync("example/repo", request);

        result.Id.Should().Be(123);
        result.Title.Should().Be("Test title");
        result.State.Should().Be(IssueState.Open);
        result.Url.Should().Be(gitlabResponse.web_url);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public async Task CreateIssueAsync_WhenApiFails_ThrowsHttpRequestException(HttpStatusCode statusCode)
    {
        var httpResponse = new HttpResponseMessage(statusCode);
        var service = CreateService(httpResponse);
        var request = new IssueRequest { Title = "Test", Description = "Test" };

        await Assert.ThrowsAsync<HttpRequestException>(() =>
            service.CreateIssueAsync("example/repo", request));
    }

    [Fact]
    public async Task CreateIssueAsync_WhenMissingFieldsInJson_ThrowsInvalidOperationException()
    {
        var brokenJson = "{\"iid\":123,\"title\":\"Test title\"}";

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(brokenJson, Encoding.UTF8, "application/json")
        };

        var service = CreateService(httpResponse);
        var request = new IssueRequest { Title = "Test", Description = "Test" };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateIssueAsync("example/repo", request));
    }

    [Fact]
    public async Task CreateIssueAsync_WhenUnknownState_ThrowsInvalidOperationException()
    {
        var gitlabResponse = new
        {
            iid = 123,
            title = "Test title",
            state = "in_progress",
            web_url = "https://gitlab.com/example/repo/-/issues/123"
        };

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(gitlabResponse), Encoding.UTF8, "application/json")
        };

        var service = CreateService(httpResponse);
        var request = new IssueRequest { Title = "Test", Description = "Test" };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateIssueAsync("example/repo", request));
    }

    [Fact]
    public async Task CreateIssueAsync_WhenFieldsAreNull_SerializesCorrectly()
    {
        var gitlabResponse = new
        {
            iid = 1,
            title = (string?)null,
            state = "closed",
            web_url = "https://gitlab.com/example/repo/-/issues/1"
        };

        var httpResponse = new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent(JsonSerializer.Serialize(gitlabResponse), Encoding.UTF8, "application/json")
        };

        var service = CreateService(httpResponse);
        var request = new IssueRequest { Title = null, Description = null };

        var result = await service.CreateIssueAsync("example/repo", request);

        result.Id.Should().Be(gitlabResponse.iid);
        result.Title.Should().BeNull();
        result.State.Should().Be(IssueState.Closed);
        result.Url.Should().Be(gitlabResponse.web_url);
    }

    [Fact]
    public async Task UpdateIssueAsync_WhenSuccess_ReturnsCorrectIssueResponse()
    {
        var gitlabResponse = new
        {
            iid = 321,
            title = "Updated title",
            state = "closed",
            web_url = "https://gitlab.com/example/repo/-/issues/321"
        };

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(gitlabResponse), Encoding.UTF8, "application/json")
        };

        var service = CreateService(httpResponse);
        var request = new IssueRequest { Title = "Updated title", Description = "Updated description" };

        var result = await service.UpdateIssueAsync("example/repo", 321, request);

        result.Id.Should().Be(gitlabResponse.iid);
        result.Title.Should().Be(gitlabResponse.title);
        result.State.Should().Be(HelpersMethod.MapState(gitlabResponse.state));
        result.Url.Should().Be(gitlabResponse.web_url);
    }

    [Fact]
    public async Task CloseIssueAsync_WhenSuccess_DoesNotThrow()
    {
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK);

        var service = CreateService(httpResponse);

        var act = async () => await service.CloseIssueAsync("example/repo", 999);

        await act.Should().NotThrowAsync();
    }
}
