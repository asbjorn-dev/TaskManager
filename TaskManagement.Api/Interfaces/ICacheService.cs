using System;

namespace TaskManagement.Api.Interfaces;

public interface ICacheService
{
    // Get cached item - returner null hvis ingen findes
    Task<T?> GetAsync<T>(string key);
    // Cache en item med expiration time
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    // fjern en speicifik cached item
    Task RemoveAsync(string key);
    // fjern alle cached items der starter med en prefix (f.eks. "tasks:" removes "tasks:all, "tasks:5")
    Task RemoveByPrefixAsync(string prefix);
}
