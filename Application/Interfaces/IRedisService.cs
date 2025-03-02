namespace PaymentsMS.Application.Interfaces
{
    public interface IRedisService
    {
        void SetValue(string key, string value, TimeSpan expiry);
        string? GetValue(string key);
        void DeleteKey(string key);
    }
}
