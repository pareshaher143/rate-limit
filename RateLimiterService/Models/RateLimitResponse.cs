namespace RateLimiterService.Models;

public class RateLimitResponse
{
    public bool Allowed { get; set; }
    public int RemainingRequests { get; set; }
    public DateTime ResetTime { get; set; }
}