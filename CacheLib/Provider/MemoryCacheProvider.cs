using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;
using CacheLib.Model;
using StackExchange.Redis;

namespace CacheLib.Provider
{
    public class MemoryCacheProvider : ICacheProvider
    {
        /// <summary>
        /// Cache
        /// </summary>
        private ObjectCache _memoryCache;

        /// <summary>
        /// Expire Time
        /// </summary>
        private TimeSpan _defaultExpireTime { get; set; }

        /// <summary>
        /// Init MemoryCacheProvider by specify memoryCache
        /// </summary>
        /// <param name="server"></param>
        /// <param name="database"></param>
        /// <param name="defalutExpireTime"></param>
        public MemoryCacheProvider(ObjectCache memoryCache, int defaultExpireTime)
        {
            this._memoryCache = memoryCache;
            this._defaultExpireTime = TimeSpan.FromSeconds(defaultExpireTime);
        }

        public bool IsExist(string key)
        {
            return this._memoryCache.Contains(key);
        }

        public string? Get(string key, CommandFlags flags = CommandFlags.None)
        {
            return this.Get<string>(key);
        }

        public T? Get<T>(string key, CommandFlags flags = CommandFlags.None) where T : class
        {
            var memoryCacheObject = this._memoryCache.Get(key);
            var result = (T)Convert.ChangeType(memoryCacheObject, typeof(T));

            return result;
        }

        public Task<string?> GetAsync(string key, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        public Task<T?> GetAsync<T>(string key, CommandFlags flags = CommandFlags.None) where T : class
        {
            throw new NotImplementedException();
        }

        public bool Set(string key, string value, CommandFlags flags = CommandFlags.None, TimeSpan? expireTime = null)
        {
            return this.Set<string>(key, value, flags, expireTime);
        }

        public bool Set<T>(string key, T value, CommandFlags flags = CommandFlags.None, TimeSpan? expireTime = null) where T : class
        {
            try
            {
                if (expireTime.HasValue == false)
                {
                    expireTime = _defaultExpireTime;
                }

                var absoluteExpiration = DateTimeOffset.Now.AddTicks(expireTime.Value.Ticks);

                this._memoryCache.Set(key, value, absoluteExpiration);
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        public void Remove(string key, CommandFlags flags = CommandFlags.None)
        {
            this._memoryCache.Remove(key);
        }

        public long IncrBy(string key, long increaseVal, CommandFlags flags = CommandFlags.None, TimeSpan? expireTime = null)
        {
            throw new NotImplementedException();
        }

        public List<string> GetKeyList(int db, string pattern, int pageSize, int cursor, int offset, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        public string? Get(string key, TimeSpan expireTime, CommandFlags flags = CommandFlags.None)
        {
            throw new NotImplementedException();
        }

        public T? Get<T>(string key, TimeSpan expireTime, CommandFlags flags = CommandFlags.None) where T : class
        {
            throw new NotImplementedException();
        }

        public CacheEntity<T> GetWithExpireTime<T>(string key, CommandFlags flags = CommandFlags.None) where T : class
        {
            throw new NotImplementedException();
        }

        public List<T?>? StringBatchGet<T>(List<string> cacheKeys, int batchSize) where T : class
        {
            throw new NotImplementedException();
        }

        public List<string?> StringBatchGet(List<string> cacheKeys, int batchSize = 10)
        {
            throw new NotImplementedException();
        }

        public int StringBatchSet<T>(List<KeyValuePair<string, T>> cacheKeysValue, int batchSize = 10, TimeSpan? expireTime = null) where T : class
        {
            throw new NotImplementedException();
        }
    }
}
