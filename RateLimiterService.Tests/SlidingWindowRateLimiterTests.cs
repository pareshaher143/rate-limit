namespace RateLimiterService.Tests;

using Microsoft.Extensions.Options;
using RateLimiterService.Configuration;
using RateLimiterService.Interfaces;
using RateLimiterService.Services;
using Xunit;

public class SlidingWindowRateLimiterTests
{
    private IRateLimiterService CreateRateLimiter(int requestLimit = 5, int windowSize = 60)
    {
        var options = Options.Create(new RateLimitOptions
        {
            RequestLimit = requestLimit,
            WindowSizeInSeconds = windowSize
        });
        var store = new InMemoryRateLimitStore();
        return new SlidingWindowRateLimiter(store, options);
    }

    [Fact]
    public async Task CheckRateLimit_FirstRequest_ShouldAllow()
    {
        // Arrange
        var rateLimiter = CreateRateLimiter();
        var identifier = "user123";

        // Act
        var result = await rateLimiter.CheckRateLimitAsync(identifier);

        // Assert
        Assert.True(result.Allowed);
        Assert.Equal(4, result.RemainingRequests);
    }

    [Fact]
    public async Task CheckRateLimit_WithinLimit_ShouldAllow()
    {
        // Arrange
        var rateLimiter = CreateRateLimiter(requestLimit: 5);
        var identifier = "user123";

        // Act
        for (int i = 0; i < 5; i++)
        {
            var result = await rateLimiter.CheckRateLimitAsync(identifier);
            Assert.True(result.Allowed);
        }
    }

    [Fact]
    public async Task CheckRateLimit_ExceedLimit_ShouldDeny()
    {
        // Arrange
        var rateLimiter = CreateRateLimiter(requestLimit: 3);
        var identifier = "user123";

        // Act - Make 3 allowed requests
        for (int i = 0; i < 3; i++)
        {
            await rateLimiter.CheckRateLimitAsync(identifier);
        }

        // Act - 4th request should be denied
        var result = await rateLimiter.CheckRateLimitAsync(identifier);

        // Assert
        Assert.False(result.Allowed);
        Assert.Equal(0, result.RemainingRequests);
    }

    [Fact]
    public async Task CheckRateLimit_DifferentIdentifiers_ShouldBeIndependent()
    {
        // Arrange
        var rateLimiter = CreateRateLimiter(requestLimit: 2);

        // Act & Assert
        var result1 = await rateLimiter.CheckRateLimitAsync("user1");
        var result2 = await rateLimiter.CheckRateLimitAsync("user2");

        Assert.True(result1.Allowed);
        Assert.True(result2.Allowed);
        Assert.Equal(1, result1.RemainingRequests);
        Assert.Equal(1, result2.RemainingRequests);
    }

    [Fact]
    public async Task CheckRateLimit_AfterWindowExpires_ShouldResetLimit()
    {
        // Arrange
        var rateLimiter = CreateRateLimiter(requestLimit: 2, windowSize: 1);
        var identifier = "user123";

        // Act - Use up the limit
        await rateLimiter.CheckRateLimitAsync(identifier);
        await rateLimiter.CheckRateLimitAsync(identifier);

        // Wait for window to expire
        await Task.Delay(1100);

        // Try again after window expires
        var result = await rateLimiter.CheckRateLimitAsync(identifier);

        // Assert
        Assert.True(result.Allowed);
    }
}