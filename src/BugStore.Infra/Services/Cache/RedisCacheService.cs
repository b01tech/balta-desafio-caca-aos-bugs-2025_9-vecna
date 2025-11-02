using System.Text.Json;
using BugStore.Application.Services.Cache;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace BugStore.Infra.Services.Cache;

public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _distributedCache;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public RedisCacheService(IDistributedCache distributedCache, ILogger<RedisCacheService> logger)
    {
        _distributedCache = distributedCache;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        where T : class
    {
        try
        {
            var cachedValue = await _distributedCache.GetStringAsync(key, cancellationToken);

            if (string.IsNullOrEmpty(cachedValue))
            {
                _logger.LogDebug("Cache miss for key: {Key}", key);
                return null;
            }

            _logger.LogDebug("Cache hit for key: {Key}", key);
            return JsonSerializer.Deserialize<T>(cachedValue, _jsonOptions);
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cache for key: {Key}", key);
            return null;
        }
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        CancellationToken cancellationToken = default
    )
        where T : class
    {
        try
        {
            var serializedValue = JsonSerializer.Serialize(value, _jsonOptions);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
            };

            await _distributedCache.SetStringAsync(
                key,
                serializedValue,
                options,
                cancellationToken
            );
            _logger.LogDebug("Cache set for key: {Key}", key);
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Error setting cache for key: {Key}", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _distributedCache.RemoveAsync(key, cancellationToken);
            _logger.LogDebug("Cache removed for key: {Key}", key);
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Error removing cache for key: {Key}", key);
        }
    }
}
