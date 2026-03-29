using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Hospital.Web.Infrastructure.Caching;

/// <summary>
/// Redis-backed cache using IDistributedCache + JSON serialisation.
/// Sync methods block on async — safe on ASP.NET Core thread-pool threads (no sync context).
/// All methods swallow exceptions and return null/default so the app degrades gracefully.
/// </summary>
public sealed class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<RedisCacheService> _log;
    private static readonly TimeSpan DefaultExpiry = TimeSpan.FromMinutes(5);

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    public RedisCacheService(IDistributedCache cache, ILogger<RedisCacheService> log)
    {
        _cache = cache;
        _log   = log;
    }

    // ── Async ─────────────────────────────────────────────────────────────────

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        try
        {
            var bytes = await _cache.GetAsync(key, ct);
            if (bytes == null || bytes.Length == 0) return default;
            return JsonSerializer.Deserialize<T>(bytes, JsonOpts);
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "[Cache] GET failed key='{Key}'", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken ct = default)
    {
        try
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(value, JsonOpts);
            var opts  = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiry ?? DefaultExpiry
            };
            await _cache.SetAsync(key, bytes, opts, ct);
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "[Cache] SET failed key='{Key}'", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken ct = default)
    {
        try { await _cache.RemoveAsync(key, ct); }
        catch (Exception ex) { _log.LogWarning(ex, "[Cache] REMOVE failed key='{Key}'", key); }
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory,
        TimeSpan? expiry = null, CancellationToken ct = default)
    {
        var cached = await GetAsync<T>(key, ct);
        if (cached != null) return cached;
        var value = await factory();
        await SetAsync(key, value, expiry, ct);
        return value;
    }

    // ── Sync (blocks on async — safe in ASP.NET Core, no deadlock) ───────────

    public T? Get<T>(string key) => GetAsync<T>(key).GetAwaiter().GetResult();

    public void Set<T>(string key, T value, TimeSpan? expiry = null)
        => SetAsync(key, value, expiry).GetAwaiter().GetResult();

    public void Remove(string key) => RemoveAsync(key).GetAwaiter().GetResult();

    public T GetOrSet<T>(string key, Func<T> factory, TimeSpan? expiry = null)
    {
        var cached = Get<T>(key);
        if (cached != null) return cached;
        var value = factory();
        Set(key, value, expiry);
        return value;
    }
}
