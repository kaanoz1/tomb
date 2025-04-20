using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileSystemGlobbing;
using tomb.DB;
using tomb.Model;

namespace tomb.Services
{
    public interface ICacheService
    {
        Task<T?> GetCachedDataAsync<T>(string key);
        Task SetCacheDataAsync<T>(string key, T data, TimeSpan? validFor = null);
    }

    public class CacheService(ApplicationDBContext db) : ICacheService
    {
        private readonly ApplicationDBContext _db = db ?? throw new ArgumentNullException(nameof(db));

        public async Task<T?> GetCachedDataAsync<T>(string key)
        {
            Cache? cache = await _db.Caches
                .Where(c => c.Key == key && c.ExpirationDate > DateTime.UtcNow)
                .FirstOrDefaultAsync();

            if (cache == null) return default;

            try
            {
                T data = JsonSerializer.Deserialize<T>(cache.Data)!;

                return data;
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Deserialization error: {ex.Message}");
                return default;
            }
        }


        public async Task SetCacheDataAsync<T>(string key, T data, TimeSpan? validFor = null)
        {
            string jsonData = JsonSerializer.Serialize(data);

            Cache? existingCacheEntry = await _db.Caches.FirstOrDefaultAsync(c => c.Key == key);

            if (existingCacheEntry != null)
            {
                existingCacheEntry.Data = jsonData;
                _db.Caches.Update(existingCacheEntry);
            }
            else
            {
                TimeSpan existingPeriod = validFor ?? TimeSpan.FromMinutes(5);

                var cacheEntry = new Cache
                {
                    Key = key,
                    Data = jsonData,
                    ExpirationDate = DateTime.UtcNow + existingPeriod
                };

                _db.Caches.Add(cacheEntry);

            }

            await _db.SaveChangesAsync();
        }


    }
}
