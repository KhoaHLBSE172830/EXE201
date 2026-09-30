using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Mimo.Application.Services;
using StackExchange.Redis;

namespace Mimo.Infrastructure.Core.Cache;

public class CacheService : ICacheService
{
    private readonly IDatabase _database;
    private readonly string _redisKey = "mimo";

    public CacheService(IConnectionMultiplexer connectionMultiplexer)
    {
        _database = connectionMultiplexer.GetDatabase();
    }

    public async Task<T?> Get<T>(string key)
    {
        var value = await _database.StringGetAsync($"{_redisKey}:{key}");
        if (value.IsNullOrEmpty)
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>((string)value!);
    }

    public Task Set<T>(string key, T value)
    {
        return _database.StringSetAsync($"{_redisKey}:{key}", JsonSerializer.Serialize(value), TimeSpan.FromMinutes(30));
    }

    public Task Set<T>(string key, T value, TimeSpan expiration)
    {
        return _database.StringSetAsync($"{_redisKey}:{key}", JsonSerializer.Serialize(value), expiration);
    }

    public async Task<bool> Update<T>(string key, T value)
    {
        TimeSpan? ttl = await _database.KeyTimeToLiveAsync($"{_redisKey}:{key}");
        return await _database.StringSetAsync($"{_redisKey}:{key}", JsonSerializer.Serialize(value), ttl);
    }

    public Task Remove(string key)
    {
        return _database.KeyDeleteAsync($"{_redisKey}:{key}");
    }

    public Task<bool> Exists(string key)
    {
        return _database.KeyExistsAsync($"{_redisKey}:{key}");
    }

    public Task Clear()
    {
        return _database.ExecuteAsync("FLUSHDB");
    }

    public Task ClearWithPattern(string pattern)
    {
        var endpoints = _database.Multiplexer.GetEndPoints();
        if (endpoints.Length == 0) return Task.CompletedTask;

        var server = _database.Multiplexer.GetServer(endpoints.First());
        var keys = server.Keys(pattern: $"{_redisKey}:{pattern}*").ToArray();
        if (keys.Length == 0) return Task.CompletedTask;

        return _database.KeyDeleteAsync(keys);
    }

    public Task ForceLogout(Guid userId)
    {
        var redisKey = $"ss:{userId}";
        return ClearWithPattern(redisKey);
    }
}
