# Rate Limiter Service

A high-performance ASP.NET Core Web API implementing a sliding window rate limiting algorithm to control API request rates.

---

## 📋 Project Overview

This project is a **RESTful API service** that provides rate limiting functionality using a **sliding window algorithm**. The service allows clients to check whether their requests should be allowed or rejected based on configurable rate limits.

### Key Features

- ✅ **Sliding Window Rate Limiting**: Provides accurate, fair rate limiting that prevents burst traffic at window boundaries
- ✅ **Thread-Safe In-Memory Storage**: Uses concurrent collections for high-performance, multi-threaded access
- ✅ **Pluggable Storage Design**: Easy to swap in-memory store with Redis, SQL, or other backends
- ✅ **RESTful API**: Simple, clean API with proper HTTP status codes and headers
- ✅ **Standard Rate Limit Headers**: Returns `X-RateLimit-Limit`, `X-RateLimit-Remaining`, and `X-RateLimit-Reset`
- ✅ **Configurable Limits**: Externalized configuration via `appsettings.json`
- ✅ **Comprehensive Testing**: Unit and integration tests with xUnit
- ✅ **Swagger/OpenAPI**: Interactive API documentation

### Technology Stack

- **.NET 10.0**
- **ASP.NET Core Web API**
- **Swagger/OpenAPI** for API documentation
- **xUnit** for testing
- **Dependency Injection** for loose coupling

## Project Structure

```
RateLimiterService/
├── Configuration/
│   └── RateLimitOptions.cs          # Rate limit configuration model
├── Controllers/
│   └── RateLimitController.cs       # API endpoints
├── Interfaces/
│   ├── IRateLimiterService.cs       # Rate limiter service interface
│   └── IRateLimitStore.cs           # Storage interface
├── Models/
│   ├── ExistingRateLimitInfo.cs     # Request tracking model
│   ├── RateLimitRequest.cs          # API request model
│   └── RateLimitResponse.cs         # API response model
├── Services/
│   ├── InMemoryRateLimitStore.cs    # In-memory storage implementation
│   └── SlidingWindowRateLimiter.cs  # Sliding window algorithm implementation
└── Program.cs                        # Application entry point

RateLimiterService.Tests/
├── SlidingWindowRateLimiterTests.cs          # Unit tests
└── RateLimitControllerIntegrationTests.cs    # Integration tests
```


---

## 🚀 Setup and Execution

Follow these step-by-step instructions to build, configure, and run the service locally.

### Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download) or later
- IDE: Visual Studio 2022, VS Code, or JetBrains Rider (optional)
- Terminal/Command Prompt

### Step 1: Clone the Repository

```bash
git clone <repository-url>
cd Rate-Limit
```

### Step 2: Restore Dependencies

```bash
dotnet restore
```

This will restore all NuGet packages required by the project.

### Step 3: Configure the Service

Edit `RateLimiterService/appsettings.json` to configure rate limiting parameters:

```json
{
  "RateLimit": {
    "RequestLimit": 100,
    "WindowSizeInSeconds": 60
  }
}
```

**Configuration Options:**
- `RequestLimit`: Maximum number of requests allowed within the time window (default: 100)
- `WindowSizeInSeconds`: Duration of the sliding window in seconds (default: 60)

### Step 4: Build the Solution

```bash
dotnet build
```

This compiles the solution and checks for any compilation errors.

### Step 5: Run the Service

```bash
cd RateLimiterService
dotnet run
```

The service will start and display output similar to:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7001
      Now listening on: http://localhost:5001
```

### Step 6: Access Swagger UI

Open your browser and navigate to:
```
https://localhost:7001/swagger
```

This provides an interactive API documentation interface where you can test endpoints.

### Running Tests

#### Run All Tests
```bash
dotnet test
```

#### Run Tests with Detailed Output
```bash
dotnet test --verbosity detailed
```

#### Run Tests with Code Coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

#### Run Specific Test Project
```bash
dotnet test RateLimiterService.Tests/RateLimiterService.Tests.csproj
```

### Troubleshooting

**Port Already in Use:**
- Edit `RateLimiterService/Properties/launchSettings.json` to change the port numbers

**Build Errors:**
- Ensure .NET 10.0 SDK is installed: `dotnet --version`
- Clean and rebuild: `dotnet clean && dotnet build`

---


## 📚 API Documentation

The service exposes a simple RESTful API for checking rate limits.

### Endpoint: POST /api/check

Checks if a request from the specified identifier should be allowed based on current rate limits.

#### Request

**URL:** `POST /api/check`

**Headers:**
```
Content-Type: application/json
```

**Request Body:**
```json
{
  "identifier": "user123"
}
```

**Parameters:**
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| identifier | string | Yes | Unique identifier for the client (e.g., user ID, API key, IP address) |

#### Responses

##### Success Response (200 OK)

**Condition:** Request is allowed (under rate limit)

**Response Body:**
```json
{
  "allowed": true,
  "remainingRequests": 95,
  "resetTime": "2026-01-02T10:15:30.123Z"
}
```

**Response Headers:**
```
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 95
X-RateLimit-Reset: 2026-01-02T10:15:30.123Z
```

**Response Fields:**
| Field | Type | Description |
|-------|------|-------------|
| allowed | boolean | Whether the request is allowed (true) |
| remainingRequests | integer | Number of remaining requests in the current window |
| resetTime | datetime | ISO 8601 timestamp when the oldest request expires |

---

##### Rate Limit Exceeded (429 Too Many Requests)

**Condition:** Request exceeds rate limit

**Response Body:**
```json
{
  "error": "Too many requests",
  "resetTime": "2026-01-02T10:15:30.123Z"
}
```

**Response Headers:**
```
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 0
X-RateLimit-Reset: 2026-01-02T10:15:30.123Z
```

---

##### Bad Request (400 Bad Request)

**Condition:** Missing or invalid identifier

**Response Body:**
```json
{
  "error": "Identifier is required"
}
```

---

### Example Usage

#### Using cURL

```bash
curl -X POST https://localhost:7001/api/check \
  -H "Content-Type: application/json" \
  -d '{"identifier": "user123"}'
```

#### Using PowerShell

```powershell
$body = @{
    identifier = "user123"
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://localhost:7001/api/check" `
  -Method Post `
  -ContentType "application/json" `
  -Body $body
```

#### Using C#

```csharp
using var client = new HttpClient();
var request = new RateLimitRequest { Identifier = "user123" };
var response = await client.PostAsJsonAsync(
    "https://localhost:7001/api/check", 
    request
);
var result = await response.Content.ReadFromJsonAsync<RateLimitResponse>();
```

---


## 🏗️ Design Decisions & Trade-offs

This section details the architectural choices, algorithms, and trade-offs made in building this rate limiting service.

---

### 1. Rate Limiting Algorithm: Sliding Window

#### Why Sliding Window?

I chose the **Sliding Window** algorithm over other common approaches (Fixed Window, Token Bucket, Leaky Bucket) for the following reasons:

**Advantages:**
- ✅ **Accuracy**: Provides the most precise rate limiting by tracking individual request timestamps
- ✅ **Fairness**: Prevents the "burst at window boundary" problem common in fixed window approaches
- ✅ **Simplicity**: Easier to understand and reason about than token bucket algorithms
- ✅ **Predictability**: Reset time is clearly defined (when the oldest request expires)

**The Fixed Window Problem:**
```
Fixed Window (100 req/min):
Window 1: [09:00:00 - 09:00:59] -> 100 requests at 09:00:59 ✓
Window 2: [09:01:00 - 09:01:59] -> 100 requests at 09:01:00 ✓
Result: 200 requests in 2 seconds! ❌

Sliding Window:
At 09:01:00, window looks back 60 seconds to 09:00:00
Sees 100 requests from previous window -> BLOCKS new requests ✓
```

**How It Works:**
1. For each request, calculate window start: `now - windowSize`
2. Remove all requests older than window start
3. Count remaining requests in the window
4. If count < limit, allow request and record timestamp
5. If count >= limit, reject and return reset time (oldest request + window size)

**Trade-offs:**
- ❌ **Memory Usage**: Stores individual timestamps (more memory than fixed window counters)
- ❌ **Cleanup Overhead**: Requires periodic cleanup of old timestamps
- ✅ **Worth It**: The accuracy and fairness benefits outweigh memory costs for most use cases

---

### 2. Code Structure & Architecture

#### Layered Architecture with Dependency Injection

The project follows **Clean Architecture** principles with clear separation of concerns:

```
┌─────────────────────────────────────────┐
│         Controllers Layer               │
│  (HTTP requests/responses, validation)  │
└──────────────┬──────────────────────────┘
               │
               ▼
┌─────────────────────────────────────────┐
│         Services Layer                  │
│  (Business logic, rate limiting algo)   │
└──────────────┬──────────────────────────┘
               │
               ▼
┌─────────────────────────────────────────┐
│         Storage Layer                   │
│  (Data persistence abstraction)         │
└─────────────────────────────────────────┘
```

#### Key Design Patterns

**1. Interface Segregation**
```csharp
// Storage abstraction
public interface IRateLimitStore
{
    Task<ExistingRateLimitInfo> GetRequestInfoAsync(string identifier);
    Task AddRequestAsync(string identifier, DateTime timestamp);
    Task RemoveOldRequestsAsync(string identifier, DateTime cutoffTime);
}

// Algorithm abstraction
public interface IRateLimiterService
{
    Task<RateLimitResponse> CheckRateLimitAsync(string identifier);
}
```

**Why?**
- Makes implementation details swappable
- Enables easy testing with mocks
- Clear contracts between layers

**2. Dependency Injection**
```csharp
// Registered in Program.cs
builder.Services.AddSingleton<IRateLimitStore, InMemoryRateLimitStore>();
builder.Services.AddScoped<IRateLimiterService, SlidingWindowRateLimiter>();
```

**Why?**
- Loose coupling between components
- Easy to swap implementations
- Testability through constructor injection

**3. Options Pattern**
```csharp
public class RateLimitOptions
{
    public const string SectionName = "RateLimit";
    public int RequestLimit { get; set; } = 100;
    public int WindowSizeInSeconds { get; set; } = 60;
}
```

**Why?**
- Configuration is externalized
- Type-safe configuration
- Supports multiple environments

---

### 3. Trade-offs Made

#### Performance vs. Accuracy

**Decision:** Chose accuracy over raw performance

**Trade-off:**
- ✅ **Accurate**: Each request timestamp is stored and tracked
- ❌ **Memory**: Uses more memory than counter-based approaches
- ❌ **CPU**: Cleanup operations require iterating timestamps

**Justification:**
- For most APIs, memory cost is negligible (timestamps are small)
- Accuracy prevents abuse and ensures fairness
- Cleanup is async and doesn't block request processing

**Alternatives Considered:**
- **Fixed Window**: Faster but allows burst traffic (rejected)
- **Token Bucket**: More complex, harder to explain reset time (rejected)

---

#### Simplicity vs. Feature-Richness

**Decision:** Start simple, design for extensibility

**What I Kept Simple:**
- Single global rate limit configuration
- In-memory storage only
- One endpoint (`/api/check`)

**What I Designed for Extension:**
- Interface-based architecture allows adding features later
- Storage abstraction enables distributed scenarios
- Algorithm abstraction allows multiple strategies

**Features Not Implemented (Yet):**
- Multiple rate limit tiers (e.g., per-user, per-endpoint)
- Distributed rate limiting with Redis
- Rate limit rules engine
- Admin UI for monitoring

**Justification:**
- YAGNI principle: Don't build what you don't need yet
- Clear interfaces make adding features straightforward
- Keeps initial implementation testable and maintainable

---

#### Thread Safety Approach

**Decision:** Use `SemaphoreSlim` with `ConcurrentDictionary`

**Code:**
```csharp
private readonly ConcurrentDictionary<string, List<DateTime>> _requestStore = new();
private readonly SemaphoreSlim _semaphore = new(1, 1);

public async Task RemoveOldRequestsAsync(string identifier, DateTime cutoffTime)
{
    await _semaphore.WaitAsync();
    try
    {
        if (_requestStore.TryGetValue(identifier, out var requests))
        {
            _requestStore[identifier] = requests.Where(r => r >= cutoffTime).ToList();
            // ...
        }
    }
    finally
    {
        _semaphore.Release();
    }
}
```

**Why This Approach?**
- `ConcurrentDictionary`: Thread-safe dictionary operations
- `SemaphoreSlim`: Protects list mutations (add, remove)
- `async/await`: Non-blocking for better scalability

**Trade-off:**
- ✅ **Safe**: No race conditions
- ❌ **Bottleneck**: Global semaphore can serialize access
- ⚖️ **Acceptable**: For single-instance deployments, this is sufficient

**Better Approach for Scale:**
- Use per-identifier locks (finer-grained locking)
- Or switch to Redis with atomic operations

---

### 4. How the Design Allows Storage Replacement

The design uses the **Strategy Pattern** with dependency injection, making it trivial to swap storage implementations.

#### Current Architecture

```csharp
// 1. Interface defines the contract
public interface IRateLimitStore
{
    Task<ExistingRateLimitInfo> GetRequestInfoAsync(string identifier);
    Task AddRequestAsync(string identifier, DateTime timestamp);
    Task RemoveOldRequestsAsync(string identifier, DateTime cutoffTime);
}

// 2. In-memory implementation
public class InMemoryRateLimitStore : IRateLimitStore { /* ... */ }

// 3. Algorithm depends on interface, not implementation
public class SlidingWindowRateLimiter : IRateLimiterService
{
    private readonly IRateLimitStore _store;  // ← Interface dependency
    
    public SlidingWindowRateLimiter(IRateLimitStore store, /* ... */)
    {
        _store = store;
    }
}

// 4. DI container wires it up
builder.Services.AddSingleton<IRateLimitStore, InMemoryRateLimitStore>();
```

#### How to Replace with Redis

**Step 1:** Create `RedisRateLimitStore.cs`

```csharp
public class RedisRateLimitStore : IRateLimitStore
{
    private readonly IConnectionMultiplexer _redis;
    
    public RedisRateLimitStore(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }
    
    public async Task<ExistingRateLimitInfo> GetRequestInfoAsync(string identifier)
    {
        var db = _redis.GetDatabase();
        var key = $"ratelimit:{identifier}";
        
        // Use Redis Sorted Set to store timestamps
        var count = await db.SortedSetLengthAsync(key);
        var oldest = await db.SortedSetRangeByScoreAsync(key, take: 1);
        
        return new ExistingRateLimitInfo
        {
            Count = (int)count,
            OldestRequestTime = oldest.Any() 
                ? DateTimeOffset.FromUnixTimeMilliseconds((long)oldest[0]).DateTime 
                : DateTime.MinValue
        };
    }
    
    public async Task AddRequestAsync(string identifier, DateTime timestamp)
    {
        var db = _redis.GetDatabase();
        var key = $"ratelimit:{identifier}";
        var score = new DateTimeOffset(timestamp).ToUnixTimeMilliseconds();
        
        await db.SortedSetAddAsync(key, score.ToString(), score);
    }
    
    public async Task RemoveOldRequestsAsync(string identifier, DateTime cutoffTime)
    {
        var db = _redis.GetDatabase();
        var key = $"ratelimit:{identifier}";
        var cutoffScore = new DateTimeOffset(cutoffTime).ToUnixTimeMilliseconds();
        
        // Remove all entries older than cutoff
        await db.SortedSetRemoveRangeByScoreAsync(key, 0, cutoffScore);
    }
}
```

**Step 2:** Update `Program.cs` (only 1 line changes!)

```csharp
// Before:
builder.Services.AddSingleton<IRateLimitStore, InMemoryRateLimitStore>();

// After:
builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect("localhost:6379"));
builder.Services.AddSingleton<IRateLimitStore, RedisRateLimitStore>();
```

**That's it!** No changes needed to:
- `SlidingWindowRateLimiter.cs`
- `RateLimitController.cs`
- Tests (can mock `IRateLimitStore`)

**Why This Works:**
- Loose coupling through interfaces
- Dependency Inversion Principle
- Open/Closed Principle (open for extension, closed for modification)

---

### 5. Distributed Systems Approach

If I were to implement distributed rate limiting across multiple service instances, here's how I would approach it:

#### Problem Statement

With multiple instances behind a load balancer:
```
        Load Balancer
           /   |   \
       Inst1 Inst2 Inst3
         |     |     |
    In-Memory stores (isolated!)
```

Each instance has its own in-memory store, so a client could make 100 requests to each instance (300 total!) ❌

#### Solution: Centralized Storage

**Option 1: Redis (Recommended)**

**Why Redis?**
- ✅ **Atomic Operations**: `ZADD`, `ZCOUNT`, `ZREMRANGEBYSCORE` are atomic
- ✅ **Sorted Sets**: Perfect for timestamp-based sliding window
- ✅ **Performance**: Sub-millisecond latency, handles millions of ops/sec
- ✅ **TTL Support**: Automatic expiration of old data
- ✅ **High Availability**: Redis Cluster or Sentinel for fault tolerance

**Architecture:**
```
        Load Balancer
           /   |   \
       Inst1 Inst2 Inst3
         |     |     |
         └─────┼─────┘
               |
          Redis Cluster
     (Single source of truth)
```

**Implementation (as shown above):**
- Use Sorted Sets with timestamps as scores
- `ZADD` to add requests
- `ZCOUNT` to check current count
- `ZREMRANGEBYSCORE` to cleanup old requests
- All operations are atomic = no race conditions

**Redis Commands:**
```redis
# Add request
ZADD ratelimit:user123 1704197400000 "1704197400000"

# Count requests in window
ZCOUNT ratelimit:user123 1704197340000 +inf

# Remove old requests
ZREMRANGEBYSCORE ratelimit:user123 0 1704197340000

# Set expiration
EXPIRE ratelimit:user123 3600
```

---

**Option 2: Distributed Cache (SQL Server, PostgreSQL)**

**Approach:**
- Use database table with indexes on `(identifier, timestamp)`
- Atomic transactions for consistency
- Periodic cleanup jobs

**Trade-offs:**
- ❌ **Slower**: Database is slower than Redis
- ❌ **More Complex**: Requires connection pooling, retry logic
- ✅ **Durable**: Data survives restarts
- ✅ **Familiar**: Teams already use SQL databases

---

**Option 3: Hybrid Approach**

**Idea:** Local cache + Redis for synchronization

```csharp
public class HybridRateLimitStore : IRateLimitStore
{
    private readonly InMemoryRateLimitStore _localCache;
    private readonly RedisRateLimitStore _redisStore;
    
    public async Task<ExistingRateLimitInfo> GetRequestInfoAsync(string identifier)
    {
        // Try local cache first (fast path)
        var local = await _localCache.GetRequestInfoAsync(identifier);
        
        // If close to limit, check Redis (authoritative)
        if (local.Count > 90)
        {
            return await _redisStore.GetRequestInfoAsync(identifier);
        }
        
        return local;
    }
    
    // Write-through: update both local and Redis
    public async Task AddRequestAsync(string identifier, DateTime timestamp)
    {
        await Task.WhenAll(
            _localCache.AddRequestAsync(identifier, timestamp),
            _redisStore.AddRequestAsync(identifier, timestamp)
        );
    }
}
```

**Trade-offs:**
- ✅ **Fast Reads**: Most requests hit local cache
- ✅ **Accurate**: Redis ensures global consistency
- ❌ **Complex**: More moving parts, cache invalidation issues

---

#### Additional Distributed Concerns

**1. Clock Synchronization**
- Use NTP to keep server clocks in sync
- Or use Redis server time: `TIME` command

**2. Network Partitions**
- What if Redis is unreachable?
  - **Fail Open**: Allow requests (availability over consistency)
  - **Fail Closed**: Block requests (consistency over availability)
  - **Circuit Breaker**: Temporarily allow after N failures

**3. Rate Limit Coordination**
- If Redis is slow (>10ms), local cache can smooth spikes
- Use Redis Pub/Sub to broadcast rate limit changes

---

### 6. What I Would Improve with More Time

If I had additional time, here are the improvements I would make, prioritized by impact:

#### High Priority

**1. Distributed Storage with Redis**
- **Why**: Enables horizontal scaling
- **Effort**: 2-3 hours
- **Impact**: Production-ready multi-instance deployment

**2. Per-Endpoint Rate Limits**
```csharp
[RateLimit(requests: 100, windowSeconds: 60)]
public async Task<IActionResult> ExpensiveOperation() { }

[RateLimit(requests: 1000, windowSeconds: 60)]
public async Task<IActionResult> CheapOperation() { }
```
- **Why**: Different endpoints have different costs
- **Effort**: 3-4 hours (attribute-based configuration)
- **Impact**: More flexible rate limiting

**3. Rate Limit Tiers**
```json
{
  "RateLimitTiers": {
    "Free": { "RequestLimit": 100, "WindowSeconds": 60 },
    "Pro": { "RequestLimit": 1000, "WindowSeconds": 60 },
    "Enterprise": { "RequestLimit": 10000, "WindowSeconds": 60 }
  }
}
```
- **Why**: Support different user tiers
- **Effort**: 2-3 hours
- **Impact**: Monetization-ready

---

#### Medium Priority

**4. Better Monitoring & Metrics**
```csharp
_metrics.RecordRateLimitCheck(identifier, allowed);
_metrics.RecordRateLimitViolation(identifier);
```
- Export to Prometheus, Application Insights, or DataDog
- **Why**: Observability is critical in production
- **Effort**: 2-3 hours
- **Impact**: Better debugging and alerting

**5. Graceful Degradation**
```csharp
public async Task<RateLimitResponse> CheckRateLimitAsync(string identifier)
{
    try
    {
        return await _store.GetRequestInfoAsync(identifier);
    }
    catch (RedisException ex)
    {
        _logger.LogError(ex, "Redis unavailable, failing open");
        return new RateLimitResponse { Allowed = true };  // Fail open
    }
}
```
- **Why**: Service shouldn't crash if Redis is down
- **Effort**: 1-2 hours
- **Impact**: Higher availability

**6. Background Cleanup Job**
```csharp
public class RateLimitCleanupService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            await CleanupOldEntriesAsync();
            await Task.Delay(TimeSpan.FromMinutes(5), token);
        }
    }
}
```
- **Why**: Prevent memory leaks in long-running processes
- **Effort**: 1-2 hours
- **Impact**: Better memory management

---

#### Lower Priority (Nice to Have)

**7. Admin API**
```csharp
[HttpGet("admin/rate-limits/{identifier}")]
public async Task<IActionResult> GetRateLimit(string identifier) { }

[HttpDelete("admin/rate-limits/{identifier}")]
public async Task<IActionResult> ResetRateLimit(string identifier) { }
```
- **Why**: Support/ops teams need visibility and control
- **Effort**: 2-3 hours

**8. Rate Limit Dashboard**
- Web UI showing current rate limits, top offenders, statistics
- **Effort**: 1 day
- **Impact**: Better UX for administrators

**9. Adaptive Rate Limiting**
```csharp
// Automatically adjust limits based on system load
if (systemLoad > 80%)
    effectiveLimit = configuredLimit * 0.7;  // Throttle more aggressively
```
- **Effort**: 4-6 hours
- **Impact**: Better resilience under stress

**10. IP-based Automatic Rate Limiting**
- Extract IP from request headers
- Apply rate limits even without explicit identifier
- **Effort**: 2-3 hours
- **Impact**: Better DDoS protection

---

#### Refactoring & Code Quality

**11. More Comprehensive Tests**
- Load testing (can it handle 10k requests/sec?)
- Concurrency testing (race conditions?)
- Chaos testing (what if Redis crashes mid-request?)
- **Effort**: 1 day

**12. Benchmarking**
- Use BenchmarkDotNet to measure performance
- Compare algorithms (sliding window vs token bucket)
- **Effort**: 2-3 hours

**13. Docker & Kubernetes Support**
- Dockerfile for containerization
- Helm charts for K8s deployment
- **Effort**: 3-4 hours

---

### Summary of Design Philosophy

1. **Start Simple**: In-memory store, single endpoint, clear contracts
2. **Design for Extension**: Interfaces allow swapping implementations
3. **Accuracy Over Speed**: Sliding window provides fairness
4. **Testability**: Dependency injection and mocks
5. **Production Path**: Clear path to Redis for distributed scenarios

The current implementation is **production-ready for single-instance deployments** and **easily extensible to distributed systems** through the storage abstraction layer.

---


## 📁 Project Structure

```
RateLimiterService/
├── Configuration/
│   └── RateLimitOptions.cs          # Rate limit configuration model
├── Controllers/
│   └── RateLimitController.cs       # API endpoints
├── Interfaces/
│   ├── IRateLimiterService.cs       # Rate limiter service interface
│   └── IRateLimitStore.cs           # Storage interface
├── Models/
│   ├── ExistingRateLimitInfo.cs     # Request tracking model
│   ├── RateLimitRequest.cs          # API request model
│   └── RateLimitResponse.cs         # API response model
├── Services/
│   ├── InMemoryRateLimitStore.cs    # In-memory storage implementation
│   └── SlidingWindowRateLimiter.cs  # Sliding window algorithm implementation
└── Program.cs                        # Application entry point

RateLimiterService.Tests/
├── SlidingWindowRateLimiterTests.cs          # Unit tests
└── RateLimitControllerIntegrationTests.cs    # Integration tests
```

---

## 🧪 Testing

The project includes comprehensive tests:

- **Unit Tests**: Test the sliding window algorithm logic in isolation
- **Integration Tests**: Test API endpoints end-to-end with WebApplicationFactory

All tests are written using xUnit and can be run individually or as a suite.

---

## 📄 License

This project is licensed under the MIT License.

---

**Built with ❤️ using ASP.NET Core**

