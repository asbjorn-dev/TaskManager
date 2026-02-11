using System;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using TaskManagement.Api.Interfaces;

namespace TaskManagement.Api.Services;

// samlet Redis wrapper. Vi har masser C# objekter, og redis kan kun gemme i bytes
// Strategi - try-catch overalt grundet hvis redis går ned, logger vi warning men app skal fortsætte
public class CacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<CacheService> _logger;
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(10); // default cache tid hvis ikke bliver specificeret
    public CacheService(IDistributedCache cache, ILogger<CacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    // return generic type(f.eks TaskResponseDto) grundet taskService har vi GetAll(liste) og GetById (en task)
    // key parametre f.eks. "tasks:123"
    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            var json = await _cache.GetStringAsync(key);

            if (json == null)
                return default; // return default værdi for T typen (f.eks. double er 0.0)
            
            _logger.LogInformation("Cache hit for key: {Key}", key);
            
            return JsonSerializer.Deserialize<T>(json);
        }
        catch (Exception ex)
        {
            // cache fejl må aldrig break app - bare log og return null
            _logger.LogWarning(ex, "Redis error on GET {Key}", key);
            return default;
        }
    }

    // tilføjer en type til redis
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        try
        {
            var options = new DistributedCacheEntryOptions
            {
                // fortæller redis: slet key X tid fra nu ellers sæt DefaultExpiration
                AbsoluteExpirationRelativeToNow = expiration ?? DefaultExpiration
            };

            // object --> JSON grundet redis gemmer kun i bytes/strings (e.g. {"id":1,"title":"Fix login bug","status":"InProgress"})
            var json = JsonSerializer.Serialize(value);
            await _cache.SetStringAsync(key, json, options); // gem key, json(value), Expiration i redis
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis Error on SET {Key}", key);
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            await _cache.RemoveAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis error on REMOVE {Key}", key);
        }
    }

    // fjerner grupper af cache entries på en gang - Redis har ikke "collections", kun keys. Prefixes simulerer grupper
    // egnet til Tasks - når en task oprettes, vil vi slette alle task relaterede cache enttries (task:all, tasks:5)
    public async Task RemoveByPrefixAsync(string prefix)
    {
        // IDistributeCache har ingen built in prefix removal
        // for nu logger vi det
        _logger.LogWarning("Cache invalidation requested for prefix: {Prefix}", prefix);
    }
}
