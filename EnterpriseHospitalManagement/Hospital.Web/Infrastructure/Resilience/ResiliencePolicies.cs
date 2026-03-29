using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using Polly.CircuitBreaker;

namespace Hospital.Web.Infrastructure.Resilience;

/// <summary>
/// Factory for pre-built Polly resilience pipelines.
/// </summary>
public static class ResiliencePolicies
{
    /// <summary>
    /// Retry policy for transient database / network errors.
    /// 3 retries with exponential back-off: 1s → 2s → 4s + jitter.
    /// </summary>
    public static AsyncRetryPolicy DatabaseRetry(ILogger? log = null)
        => Policy
            .Handle<Microsoft.Data.SqlClient.SqlException>(ex => IsTransient(ex.Number))
            .Or<TimeoutException>()
            .Or<InvalidOperationException>() // EF connection pool exhaustion
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, attempt - 1))
                    + TimeSpan.FromMilliseconds(Random.Shared.Next(0, 300)),
                onRetry: (ex, delay, attempt, _) =>
                    log?.LogWarning(ex, "[Polly] DB retry {A}/3 after {D:N0}ms", attempt, delay.TotalMilliseconds));

    /// <summary>
    /// Circuit breaker for external HTTP calls (email, SMS APIs).
    /// Opens after 5 consecutive failures, stays open for 30 s.
    /// </summary>
    public static AsyncCircuitBreakerPolicy HttpCircuitBreaker(ILogger? log = null)
        => Policy
            .Handle<HttpRequestException>()
            .Or<TimeoutException>()
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (ex, dur) =>
                    log?.LogWarning(ex, "[Polly] Circuit OPEN for {Dur}s", dur.TotalSeconds),
                onReset: () =>
                    log?.LogInformation("[Polly] Circuit CLOSED — resuming calls"),
                onHalfOpen: () =>
                    log?.LogInformation("[Polly] Circuit HALF-OPEN — testing"));

    /// <summary>
    /// Combined retry + circuit breaker for RabbitMQ publish calls.
    /// </summary>
    public static IAsyncPolicy MessageBusPolicy(ILogger? log = null)
        => Policy.WrapAsync(
            HttpCircuitBreaker(log),
            Policy.Handle<Exception>().WaitAndRetryAsync(
                2,
                attempt => TimeSpan.FromSeconds(attempt),
                (ex, delay, attempt, _) =>
                    log?.LogWarning(ex, "[Polly] MessageBus retry {A}/2 after {D:N0}ms", attempt, delay.TotalMilliseconds)));

    /// <summary>SQL Server transient error numbers.</summary>
    private static bool IsTransient(int errorNumber) => errorNumber is
        -2      // Timeout
        or 20   // Instance not found
        or 64   // Connection error
        or 233  // Connection initialisation error
        or 10053 or 10054 or 10060 // TCP errors
        or 40197 or 40501 or 40613 // Azure SQL transient
        or 49918 or 49919 or 49920;
}
