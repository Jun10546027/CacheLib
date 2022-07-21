using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CacheLib.CustomerException;
using CacheLib.Model;
using CacheLib.Provider;
using StackExchange.Redis;

namespace CacheLib.Service
{
    public class CacheService : ICacheService
    {
        /// <summary>
        /// Gets or sets the cache provider.
        /// </summary>
        /// <value>
        /// The cache provider.
        /// </value>
        private ICacheProvider _cacheProvider { get; set; }

        /// <summary>
        /// Gets or sets the default lock time.
        /// </summary>
        /// <value>
        /// The default lock time.
        /// </value>
        private TimeSpan _defaultLockTime { get; set; }

        /// <summary>
        /// Expire Time
        /// </summary>
        /// /// <value>
        /// The default expire time.
        /// </value>
        private TimeSpan _defaultExpireTime { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheService"/> class.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <param name="defaultLockTime">The default lock time.</param>
        public CacheService(ICacheProvider provider, int defaultLockTime = 300, int defaultExpireTime = 86400)
        {
            this._cacheProvider = provider;
            this._defaultLockTime = TimeSpan.FromSeconds(defaultLockTime);
            this._defaultExpireTime = TimeSpan.FromSeconds(defaultExpireTime);
        }

        /// <summary>
        /// Get Cache
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="group"></param>
        /// <param name="feature"></param>
        /// <param name="key"></param>
        /// <param name="expireTime"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public T? Get<T>(string key, CommandFlags flags = CommandFlags.None) where T : class
        {
            var cacheData = this._cacheProvider.GetWithExpireTime<T>(key, flags);

            if (cacheData?.Expire >= TimeSpan.Zero || cacheData?.Expire.HasValue == false)
            {
                return cacheData.Data;
            }

            return default(T);
        }

        /// <summary>
        /// Get Cache and Expire
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public CacheEntity<T>? GetWithExpire<T>(string key, CommandFlags flags = CommandFlags.None) where T : class
        {
            var cacheData = this._cacheProvider.GetWithExpireTime<T>(key, flags);

            if (cacheData?.Expire >= TimeSpan.Zero || cacheData?.Expire.HasValue == false)
            {
                return cacheData;
            }

            return default(CacheEntity<T>);
        }

        /// <summary>
        /// Get Cache. If not have value will set value by key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="expireTime"></param>
        /// <param name="getDataFunc"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public T? GetOrSet<T>(string key, TimeSpan expireTime, Func<T> getDataFunc, CommandFlags flags = CommandFlags.None) where T : class
        {
            var result = this.Get<T>(key, flags);
            if (result == default(T))
            {
                return this.Set<T>(key, getDataFunc(), flags, expireTime);
            }

            return result;
        }

        /// <summary>
        /// Return value when the set successfully, otherwise throw exception
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="getDataFunc"></param>
        /// <param name="flags"></param>
        /// <param name="expireTime"></param>
        /// <returns></returns>
        /// <exception cref="SetException"></exception>
        public T? Set<T>(string key, T data, CommandFlags flags = CommandFlags.None, TimeSpan? expireTime = null) where T : class
        {
            if (expireTime == default(TimeSpan))
            {
                expireTime = this._defaultExpireTime;
            }

            var result = this._cacheProvider.Set<T>(key, data, flags, expireTime);

            if (result == false)
            {
                throw new SetException("Occur error when set data into redis");
            }

            return data;
        }

        /// <summary>
        /// Return value when the set successfully, otherwise throw exception
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="getDataFunc"></param>
        /// <param name="flags"></param>
        /// <param name="expireTime"></param>
        /// <returns></returns>
        /// <exception cref="SetException"></exception>
        public T? Set<T>(string key, Func<T> getDataFunc, CommandFlags flags = CommandFlags.None, TimeSpan? expireTime = null) where T : class
        {
            return this.Set<T>(key, getDataFunc(), flags, expireTime);
        }

        /// <summary>
        /// Get string data by batch
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKeys"></param>
        /// <param name="batchSize"></param>
        /// <returns></returns>
        public List<string?> StringBatchGet(List<string> cacheKeys, int batchSize = 10)
        {
            return this._cacheProvider.StringBatchGet(cacheKeys, batchSize);
        }

        /// <summary>
        /// Get class data by batch
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKeys"></param>
        /// <param name="batchSize"></param>
        /// <returns></returns>
        public List<T?>? StringBatchGet<T>(List<string> cacheKeys, int batchSize = 10) where T : class
        {
            return this._cacheProvider.StringBatchGet<T>(cacheKeys, batchSize);
        }

        /// <summary>
        /// Set data by batch.
        /// If success will return -1, otherwise return the index of fail KeyValuePair in input collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKeysValue"></param>
        /// <param name="batchSize"></param>
        /// <returns></returns>
        public int StringBatchSet<T>(List<KeyValuePair<string, T>> cacheKeysValue, int batchSize = 1) where T : class
        {
            return this._cacheProvider.StringBatchSet(cacheKeysValue, batchSize);
        }

        /// <summary>
        /// Use transation scope when set variable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="Value"></param>
        /// <param name="flags"></param>
        /// <param name="expireTime"></param>
        /// <returns></returns>
        public async Task<bool> SetByTransationAsync<T>(string key, T Value, CommandFlags flags = CommandFlags.None, TimeSpan? expireTime = null) where T : class
        {
            var result = await this._cacheProvider.SetByTransationAsync<T>(key, Value, flags, expireTime);
            return result;
        }
    }
}
