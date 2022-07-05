using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CacheLib.Model;
using StackExchange.Redis;

namespace CacheLib.Provider
{
    public interface ICacheProvider
    {
        /// <summary>
        /// Check Key exist or not
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool IsExist(string key);

        /// <summary>
        /// Get string data via key. If data doesn't exist the special value nil will be returned.
        /// </summary>
        /// <param name="key">Cache Key</param>
        /// <param name="flags">CommandFlags</param>
        /// <returns></returns>
        string? Get(string key, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Get string data via key. If data doesn't exist the special value nil will be returned.
        /// And refresh expire time.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="expireTime"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        string? Get(string key, TimeSpan expireTime, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Get data via key. If data doesn't exist the special value nil will be returned.
        /// </summary>
        /// <param name="key">Cache Key</param>
        /// <param name="flags">CommandFlags</param>
        /// <returns></returns>
        T? Get<T>(string key, CommandFlags flags = CommandFlags.None) where T : class;

        /// <summary>
        /// Get data via key. If data doesn't exist the special value nil will be returned.
        /// And refresh expire time.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        T? Get<T>(string key, TimeSpan expireTime, CommandFlags flags = CommandFlags.None) where T : class;

        /// <summary>
        /// Get data and expire time via key. 
        /// If data doesn't exist the special value nil will be returned.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        CacheEntity<T>? GetWithExpireTime<T>(string key, CommandFlags flags = CommandFlags.None) where T : class;

        /// <summary>
        /// Get string data via key asynchronously. If data doesn't exist the special value nil will be returned.
        /// </summary>
        /// <param name="key">Cache Key</param>
        /// <param name="flags">CommandFlags</param>
        /// <returns></returns>
        Task<string?> GetAsync(string key, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Get data via key asynchronously. If data doesn't exist the special value nil will be returned.
        /// </summary>
        /// <param name="key">Cache Key</param>
        /// <param name="flags">CommandFlags</param>
        /// <returns></returns>
        Task<T?> GetAsync<T>(string key, CommandFlags flags = CommandFlags.None) where T : class;

        /// <summary>
        /// Set string via key. True when the set successfully, otherwise false.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="flags"></param>
        bool Set(string key, string value, CommandFlags flags = CommandFlags.None, TimeSpan? expireTime = null);

        /// <summary>
        /// Set data via key. True when the set successfully, otherwise false.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="flags"></param>
        bool Set<T>(string key, T value, CommandFlags flags = CommandFlags.None, TimeSpan? expireTime = null) where T : class;

        /// <summary>
        /// Remove data via key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="flags"></param>
        void Remove(string key, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Increase value via key. If have set expire time will be update on this key, otherwise keep current expire time.  
        /// </summary>
        /// <param name="key"></param>
        /// <param name="increaseVal"></param>
        /// <param name="flags"></param>
        /// <param name="expireTime"></param>
        /// <returns>Result</returns>
        long IncrBy(string key, long increaseVal, CommandFlags flags = CommandFlags.None, TimeSpan? expireTime = null);

        /// <summary>
        /// Get Key List
        /// </summary>
        /// <param name="db"></param>
        /// <param name="pattern"></param>
        /// <param name="pageSize"></param>
        /// <param name="cursor"></param>
        /// <param name="offset"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        List<string> GetKeyList(int db, string pattern, int pageSize, int cursor, int offset, CommandFlags flags = CommandFlags.None);

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
        List<T?>? StringBatchGet<T>(List<string> cacheKeys, int batchSize) where T : class;

        /// <summary>
        /// Set data by batch.
        /// If success will return -1, otherwise return the index of fail KeyValuePair in input collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKeys"></param>
        /// <param name="batchSize"></param>
        /// <returns></returns>
        int StringBatchSet<T>(List<KeyValuePair<string, T>> cacheKeysValue, int batchSize = 10, TimeSpan? expireTime = null) where T : class;


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

        /// <summary>
        /// Use transation scope when set variable asynchronously
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="flags"></param>
        /// <param name="expireTime"></param>
        /// <returns></returns>
        Task<bool> SetByTransationAsync(string key, string? value, CommandFlags flags = CommandFlags.None, TimeSpan? expireTime = null);
    }
}
