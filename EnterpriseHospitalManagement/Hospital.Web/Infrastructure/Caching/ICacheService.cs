namespace Hospital.Web.Infrastructure.Caching;

/// <summary>
/// Abstraction over IDistributedCache (Redis) with both sync and async APIs.
/// Sync methods block safely — safe to call from thread-pool threads in ASP.NET Core.
/// Falls back gracefully (returns null/default) when Redis is unavailable.
/// </summary>
public interface ICacheService
{
    // ── Sync (for use inside synchronous service methods) ─────────────────────
    T?   Get<T>(string key);
    void Set<T>(string key, T value, TimeSpan? expiry = null);
    void Remove(string key);
    T    GetOrSet<T>(string key, Func<T> factory, TimeSpan? expiry = null);

    // ── Async (for controllers / async code) ──────────────────────────────────
    Task<T?> GetAsync<T>(string key, CancellationToken ct = default);
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken ct = default);
    Task RemoveAsync(string key, CancellationToken ct = default);
    Task<T>  GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null, CancellationToken ct = default);
}
