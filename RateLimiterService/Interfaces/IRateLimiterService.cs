namespace RateLimiterService.Interfaces;

using RateLimiterService.Models;

public interface IRateLimiterService
{
    Task<RateLimitResponse> CheckRateLimitAsync(string identifier);
}