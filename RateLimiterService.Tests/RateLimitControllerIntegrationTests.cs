namespace RateLimiterService.Tests;

using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using RateLimiterService.Models;
using Xunit;

public class RateLimitControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public RateLimitControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CheckEndpoint_ValidRequest_Returns200()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new RateLimitRequest { Identifier = "test-user-1" };

        // Act
        var response = await client.PostAsJsonAsync("/api/check", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.Contains("X-RateLimit-Limit"));
        Assert.True(response.Headers.Contains("X-RateLimit-Remaining"));
    }

    [Fact]
    public async Task CheckEndpoint_ExceedLimit_Returns429()
    {
        // Arrange
        var client = _factory.CreateClient();
        var identifier = $"test-user-{Guid.NewGuid()}";

        // Act - Make 101 requests (assuming default limit is 100)
        for (int i = 0; i < 100; i++)
        {
            await client.PostAsJsonAsync("/api/check", new RateLimitRequest { Identifier = identifier });
        }

        var response = await client.PostAsJsonAsync("/api/check", new RateLimitRequest { Identifier = identifier });

        // Assert
        Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);
    }

    [Fact]
    public async Task CheckEndpoint_EmptyIdentifier_Returns400()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new RateLimitRequest { Identifier = "" };

        // Act
        var response = await client.PostAsJsonAsync("/api/check", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}