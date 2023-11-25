using System.Collections.Concurrent;
using BitFaster.Caching;
using BitFaster.Caching.Lru;
using Microsoft.Extensions.Caching.Memory;

class Program
{
    private static IMemoryCache MemoryCache = new MemoryCache(new MemoryCacheOptions()
    {
        SizeLimit = 1000
    });
    private static ConcurrentTLru<int, string> SimpleTimeLru = new ConcurrentTLru<int, string>(1000, TimeSpan.FromSeconds(5));

    // Simulated Repo
    private static Dictionary<int, string> ValueRepository = new Dictionary<int, string>()
    {
        { 1, "One" },
        { 2, "two" },
        { 3, "Three" }
    };

    static void Main(string[] args)
    {
        ConcurrentDictionary<int, string> test = null;

        for (var i = 0; i < 30; i++)
        {
            foreach (var key in ValueRepository.Keys)
            {
                GetOrAddMemoryCache(key);
                GetOrAddConcurrentLru(key);
            }
            Thread.Sleep(TimeSpan.FromSeconds(1));
        }
    }

    public static string GetOrAddConcurrentLru(int key)
    {
        

        if (SimpleTimeLru.TryGet(key, out var value))
        {
            Console.WriteLine("LruCache: Found in Cache: " + key);
            return value;
        }
        Console.WriteLine("LruCache: Not Found in Cache: " + key);

        var repoValue = ValueRepository[key];
        SimpleTimeLru.AddOrUpdate(key, repoValue);
        return repoValue;
    }

    public static string GetOrAddMemoryCache(int key)
    {
        if (MemoryCache.TryGetValue(key, out var value))
        {
            Console.WriteLine("MemCache: Found in Cache: " + key );
            return value as string ?? string.Empty;
        }

        var repoValue = ValueRepository[key];
        using (var cacheItem = MemoryCache.CreateEntry(key))
        {
            cacheItem.Value = repoValue;
            cacheItem.Size = 1;
            cacheItem.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(5);
        }
        Console.WriteLine("MemCache: Not Found in Cache: " + key);
        return repoValue;
    }
}