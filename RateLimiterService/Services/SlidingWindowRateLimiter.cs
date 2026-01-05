namespace RateLimiterService.Services;

using Microsoft.Extensions.Options;
using RateLimiterService.Configuration;
using RateLimiterService.Interfaces;
using RateLimiterService.Models;

public class SlidingWindowRateLimiter : IRateLimiterService
{
    private readonly IRateLimitStore _store;
    private readonly RateLimitOptions _options;

    public SlidingWindowRateLimiter(
        IRateLimitStore store,
        IOptions<RateLimitOptions> options)
    {
        _store = store;
        _options = options.Value;
    }

    public async Task<RateLimitResponse> CheckRateLimitAsync(string identifier)
    {
        var now = DateTime.UtcNow;
        var windowStart = now.AddSeconds(-_options.WindowSizeInSeconds);

        // Remove old requests outside the sliding window
        await _store.RemoveOldRequestsAsync(identifier, windowStart);

        // Get current request count
        ExistingRateLimitInfo existingRateLimitInfo = await _store.GetRequestInfoAsync(identifier);

        if (existingRateLimitInfo.Count >= _options.RequestLimit)
        {
            // Calculate when the oldest request will expire
            var resetTime = existingRateLimitInfo.OldestRequestTime.AddSeconds(_options.WindowSizeInSeconds);

            return new RateLimitResponse
            {
                Allowed = false,
                RemainingRequests = 0,
                ResetTime = resetTime
            };
        }

        // Add current request
        await _store.AddRequestAsync(identifier, now);

        return new RateLimitResponse
        {
            Allowed = true,
            RemainingRequests = _options.RequestLimit - existingRateLimitInfo.Count - 1,
            ResetTime = now.AddSeconds(_options.WindowSizeInSeconds)
        };
    }
}