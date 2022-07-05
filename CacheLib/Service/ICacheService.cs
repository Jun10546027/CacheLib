using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CacheLib.Model;
using StackExchange.Redis;

namespace CacheLib.Service
{
    public interface ICacheService
    {
        /// <summary>
        /// Get Cache
        /// </summary>
        /// <typeparam name="T">Data Type</typeparam>
        /// <param name="group">Cache Group</param>
        /// <param name="feature">Cache Feature</param>
        /// <param name="key">Cache Key</param>
        /// <param name="flags">CommandFlags</param>
        /// <returns>Data</returns>
        T? Get<T>(string key, CommandFlags flags = CommandFlags.None) where T : class;

        /// <summary>
        /// Get Cache and Expire
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="group"></param>
        /// <param name="feature"></param>
        /// <param name="key"></param>
        /// <param name="expireTime"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        CacheEntity<T>? GetWithExpire<T>(string key, CommandFlags flags = CommandFlags.None) where T : class;

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
        T? Set<T>(string key, T data, CommandFlags flags = CommandFlags.None, TimeSpan? expireTime = null) where T : class;

        /// <summary>
        /// Sets the cache data. Return value when the set successfully, otherwise throw exception
        /// </summary>
        /// <typeparam name="T">data type</typeparam>
        /// <param name="cacheKey">The cache key.</param>
        /// <param name="expireTime">The expire time.</param>
        /// <param name="getDataFunc">The get data function.</param>
        /// <returns>data</returns>
        T? Set<T>(string cacheKey, Func<T> getDataFunc, CommandFlags flags = CommandFlags.None, TimeSpan? expireTime = null) where T : class;

        /// <summary>
        /// Get string data by batch
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKeys"></param>
        /// <param name="batchSize"></param>
        /// <returns></returns>
        List<string?> StringBatchGet(List<string> cacheKeys, int batchSize = 10);

        /// <summary>
        /// Get class data by batch
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKeys"></param>
        /// <param name="batchSize"></param>
        /// <returns></returns>
        List<T?>? StringBatchGet<T>(List<string> cacheKeys, int batchSize = 10) where T : class;

        /// <summary>
        /// Set data by batch.
        /// If success will return -1, otherwise return the index of fail KeyValuePair in input collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKeysValue"></param>
        /// <param name="batchSize"></param>
        /// <returns></returns>
        int StringBatchSet<T>(List<KeyValuePair<string, T>> cacheKeysValue, int batchSize = 1) where T : class;

        /// <summary>
        /// Use transation scope when set variable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="Value"></param>
        /// <param name="flags"></param>
        /// <param name="expireTime"></param>
        /// <returns></returns>
        Task<bool> SetByTransationAsync<T>(string key, T Value, CommandFlags flags = CommandFlags.None, TimeSpan? expireTime = null) where T : class;
    }
}
