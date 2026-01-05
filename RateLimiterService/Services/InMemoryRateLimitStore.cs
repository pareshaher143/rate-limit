namespace RateLimiterService.Services;

using RateLimiterService.Interfaces;
using RateLimiterService.Models;
using System.Collections.Concurrent;

public class InMemoryRateLimitStore : IRateLimitStore
{
    private readonly ConcurrentDictionary<string, List<DateTime>> _requestStore = new();
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public Task<ExistingRateLimitInfo> GetRequestInfoAsync(string identifier)
    {
        if (_requestStore.TryGetValue(identifier, out var requests) && requests.Any())
        {
            return Task.FromResult(new ExistingRateLimitInfo { Count = requests.Count, OldestRequestTime = requests.Min() });
        }

        return Task.FromResult(new ExistingRateLimitInfo { Count = 0, OldestRequestTime = DateTime.MinValue });      
    }

    public async Task AddRequestAsync(string identifier, DateTime timestamp)
    {
        await _semaphore.WaitAsync();
        try
        {
            if (!_requestStore.ContainsKey(identifier))
            {
                _requestStore[identifier] = new List<DateTime>();
            }
            _requestStore[identifier].Add(timestamp);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task RemoveOldRequestsAsync(string identifier, DateTime cutoffTime)
    {
        await _semaphore.WaitAsync();
        try
        {
            if (_requestStore.TryGetValue(identifier, out var requests))
            {
                _requestStore[identifier] = requests.Where(r => r >= cutoffTime).ToList();

                // Clean up empty entries
                if (!_requestStore[identifier].Any())
                {
                    _requestStore.TryRemove(identifier, out _);
                }
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }
}