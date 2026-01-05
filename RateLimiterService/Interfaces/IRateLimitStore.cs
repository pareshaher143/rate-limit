using RateLimiterService.Models;

namespace RateLimiterService.Interfaces;

public interface IRateLimitStore
{
    Task<ExistingRateLimitInfo> GetRequestInfoAsync(string identifier);
    Task AddRequestAsync(string identifier, DateTime timestamp);
    Task RemoveOldRequestsAsync(string identifier, DateTime cutoffTime);
}