using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using IssueManager.Core.Models;
using IssueManager.Core.Models.Enums;
using IssueManager.Core.Services;
using IssueManager.Core.Services.Helpers;
using Moq;
using Moq.Protected;

namespace IssueManager.Core.Tests.Services;

public class GitHubIssueServiceTests
{
    private static GitHubIssueService CreateService(HttpResponseMessage response)
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
            BaseAddress = new Uri("https://api.github.com")
        };

        return new GitHubIssueService(httpClient, "test-token");
    }

    [Fact]
    public async Task CreateIssueAsync_WhenSuccess_ReturnsCorrectIssueResponse()
    {
        // Arrange
        var githubResponse = new
        {
            number = 123,
            title = "Test title",
            state = "open",
            htmlUrl = "https://github.com/example/repo/issues/123"
        };

        var httpResponse = new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent(JsonSerializer.Serialize(githubResponse), Encoding.UTF8, "application/vnd.github+json")
        };

        var service = CreateService(httpResponse);
        var request = new IssueRequest { Title = "Test title", Description = "Test description" };

        // Act
        var result = await service.CreateIssueAsync("example/repo", request);

        // Assert
        Assert.Equal(githubResponse.number, result.Id);
        Assert.Equal(githubResponse.title, result.Title);
        Assert.Equal(IssueState.Open, result.State);
        Assert.Equal(githubResponse.htmlUrl, result.Url);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public async Task CreateIssueAsync_WhenApiFails_ThrowsHttpRequestException(HttpStatusCode statusCode)
    {
        // Arrange
        var httpResponse = new HttpResponseMessage(statusCode);
        var service = CreateService(httpResponse);
        var request = new IssueRequest { Title = "Test", Description = "Test" };

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() =>
            service.CreateIssueAsync("example/repo", request));
    }

    [Fact]
    public async Task CreateIssueAsync_WhenMissingFieldsInJson_ThrowsJsonException()
    {
        // Arrange
        var brokenJson = "{\"number\":123,\"title\":\"Test title\"}";
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(brokenJson, Encoding.UTF8, "application/vnd.github+json")
        };

        var service = CreateService(httpResponse);
        var request = new IssueRequest { Title = "Test", Description = "Test" };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateIssueAsync("example/repo", request));
    }

    [Fact]
    public async Task CreateIssueAsync_WhenUnknownState_ThrowsInvalidOperationException()
    {
        // Arrange
        var githubResponse = new
        {
            number = 123,
            title = "Test title",
            state = "in_progress", // unknown state
            html_url = "https://github.com/example/repo/issues/123"
        };

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(githubResponse), Encoding.UTF8, "application/vnd.github+json")
        };

        var service = CreateService(httpResponse);
        var request = new IssueRequest { Title = "Test", Description = "Test" };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateIssueAsync("example/repo", request));
    }

    [Fact]
    public async Task CreateIssueAsync_WhenFieldsAreNull_SerializesCorrectly()
    {
        // Arrange
        var githubResponse = new
        {
            number = 1,
            title = (string?)null,
            state = "closed",
            html_url = "https://github.com/example/repo/issues/1"
        };

        var httpResponse = new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent(JsonSerializer.Serialize(githubResponse), Encoding.UTF8, "application/vnd.github+json")
        };

        var service = CreateService(httpResponse);
        var request = new IssueRequest { Title = null, Description = null };

        // Act
        var result = await service.CreateIssueAsync("example/repo", request);

        // Assert
        Assert.Equal(githubResponse.number, result.Id);
        Assert.Null(result.Title);
        Assert.Equal(IssueState.Closed, result.State);
    }

    [Fact]
    public async Task UpdateIssueAsync_WhenSuccess_ReturnsCorrectIssueResponse()
    {
        // Arrange
        var githubResponse = new
        {
            number = 321,
            title = "Updated title",
            state = "closed",
            htmlUrl = "https://github.com/example/repo/issues/321"
        };


        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(githubResponse), Encoding.UTF8, "application/vnd.github+json")
        };

        var service = CreateService(httpResponse);
        var request = new IssueRequest { Title = "Updated title", Description = "Updated description" };

        // Act
        var result = await service.UpdateIssueAsync("example/repo", 321, request);

        // Assert
        result.Id.Should().Be(githubResponse.number);
        result.Title.Should().Be(githubResponse.title);
        result.State.Should().Be(HelpersMethod.MapState(githubResponse.state));
        result.Url.Should().Be(githubResponse.htmlUrl);
    }

    [Fact]
    public async Task CloseIssueAsync_WhenSuccess_DoesNotThrow()
    {
        // Arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK);

        var service = CreateService(httpResponse);

        // Act
        var act = async () => await service.CloseIssueAsync("example/repo", 555);

        // Assert
        await act.Should().NotThrowAsync();
    }
}
