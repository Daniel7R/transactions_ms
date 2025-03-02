using PaymentsMS.Application.Interfaces;
using PaymentsMS.Infrastructure.Data;
using StackExchange.Redis;

namespace PaymentsMS.Application.Services
{
    public class RedisService: IRedisService
    {
        private readonly IDatabase _redisDb;

        public RedisService(RedisContext redisContext)
        {
            _redisDb = redisContext._redisDb;
        }

        public void SetValue(string key, string value, TimeSpan expiry)
        {
            _redisDb.StringSet(key, value, expiry);
        }

        public string? GetValue(string key)
        {
            return _redisDb.StringGet(key);
        }
        public void DeleteKey(string key)
        {
            _redisDb.KeyDelete(key);
        }
    }
}
