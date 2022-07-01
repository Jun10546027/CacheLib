using CacheLib;
using CacheLib.Factory;
using CacheLib.Model;
using CacheLib.Provider;
using CacheLib.Service;

// Set Connection
var connectionMutiplexer = new RedisConnManager("127.0.0.1:6379");
var lockFactory = new LockFactory(connectionMutiplexer.GetConnection());

// Create Cache Provider and Service
ICacheProvider redisCacheProvider = new RedisProvider(connectionMutiplexer.GetRedisDB(), 
                                                      lockFactory, 
                                                      lockWay: CacheLib.Enum.LockWayEnum.RedLock);
ICacheService cacheService = new CacheService(redisCacheProvider);

string testKey = "Test:Redis:Tutorial";
string testKey2 = "Test:Redis:Tutorial2";

//var setResult = cacheService.Set<string>(testKey, "1234");
//var setResult2 = cacheService.Set<string>(testKey2, "5678");

var result = cacheService.StringBatchSet<string>(new List<KeyValuePair<string, string>>()
{
    new KeyValuePair<string, string>("Test1","1"),
    new KeyValuePair<string, string>("Test2","2"),
});

CacheEntity<string>? getResult = cacheService.GetWithExpire<string>(testKey);
var getResult2 = cacheService.StringBatchGet(new List<string>() { "Test1", "Test2" });

//List<string> testSet = new List<string>() { "Test", "Test1", "Test2" };
//var testSetResult = cacheService.StringBatchGet(testSet);

Console.WriteLine("Hello, World!");


public class Setting
{
    public string Value { get; set; }

    public int Id { get; set; }
}
