namespace RateLimiterService.Models
{
    public class ExistingRateLimitInfo
    {
        public int Count { get;set; }

        public DateTime OldestRequestTime { get;set; }
    }
}
