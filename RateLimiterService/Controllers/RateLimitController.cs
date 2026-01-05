namespace RateLimiterService.Controllers;

using Microsoft.AspNetCore.Mvc;
using RateLimiterService.Interfaces;
using RateLimiterService.Models;

[ApiController]
[Route("api")]
public class RateLimitController : ControllerBase
{
    private readonly IRateLimiterService _rateLimiter;
    private readonly ILogger<RateLimitController> _logger;

    public RateLimitController(
        IRateLimiterService rateLimiter,
        ILogger<RateLimitController> logger)
    {
        _rateLimiter = rateLimiter;
        _logger = logger;
    }

    [HttpPost("check")]
    public async Task<IActionResult> Check([FromBody] RateLimitRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Identifier))
        {
            return BadRequest(new { error = "Identifier is required" });
        }

        var result = await _rateLimiter.CheckRateLimitAsync(request.Identifier);

        if (!result.Allowed)
        {
            _logger.LogWarning(
                "Rate limit exceeded for identifier: {Identifier}. Reset at: {ResetTime}",
                request.Identifier,
                result.ResetTime);

            Response.Headers.Add("X-RateLimit-Limit", "100");
            Response.Headers.Add("X-RateLimit-Remaining", "0");
            Response.Headers.Add("X-RateLimit-Reset", result.ResetTime.ToString("o"));

            return StatusCode(429, new
            {
                error = "Too many requests",
                resetTime = result.ResetTime
            });
        }

        Response.Headers.Add("X-RateLimit-Limit", "100");
        Response.Headers.Add("X-RateLimit-Remaining", result.RemainingRequests.ToString());
        Response.Headers.Add("X-RateLimit-Reset", result.ResetTime.ToString("o"));

        return Ok(new
        {
            allowed = true,
            remainingRequests = result.RemainingRequests,
            resetTime = result.ResetTime
        });
    }
}