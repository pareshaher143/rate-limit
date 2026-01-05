namespace RateLimiterService.Configuration;

public class RateLimitOptions
{
    public const string SectionName = "RateLimit";

    public int RequestLimit { get; set; } = 100;
    public int WindowSizeInSeconds { get; set; } = 60;
}