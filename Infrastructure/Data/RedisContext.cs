using StackExchange.Redis;

namespace PaymentsMS.Infrastructure.Data
{
    public class RedisContext
    {
        public readonly IDatabase _redisDb;

        public RedisContext(IConnectionMultiplexer redis)
        {
            _redisDb = redis.GetDatabase();
        }
    }
}
