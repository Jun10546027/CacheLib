using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace CacheLib.Factory
{
    public interface ILockFactory
    {
        /// <summary>
        /// Do job with Redlock
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        T? DoJobWithRedLock<T>(Func<T> job);

        /// <summary>
        /// Do job asynchronously with Redlock
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        Task<T?> DoJobWithRedLockAsync<T>(Func<Task<T>> job);

        /// <summary>
        /// Do job with Redislock
        /// </summary>
        /// <param name="key"></param>
        /// <param name="job"></param>
        /// <param name="lockExpireTime"></param>
        /// <param name="database"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        T? DoJobWithRedisLock<T>(string key, Func<T> job, TimeSpan lockExpireTime, IDatabase database, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Do job asynchronously with Redislock
        /// </summary>
        /// <param name="key"></param>
        /// <param name="job"></param>
        /// <param name="lockExpireTime"></param>
        /// <param name="database"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        Task<T?> DoJobWithRedisLockAsync<T>(string key, Func<Task<T>> job, TimeSpan lockExpireTime, IDatabase database, CommandFlags flags = CommandFlags.None);
    }
}
