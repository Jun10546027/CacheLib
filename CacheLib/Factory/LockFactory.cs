using System.Diagnostics;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;

namespace CacheLib.Factory
{
    public sealed class LockFactory : ILockFactory
    {
        private List<RedLockMultiplexer> multiplexers = new List<RedLockMultiplexer>();
        private RedLockRetryConfiguration redLockRetryConfiguration;

        public LockFactory(ConnectionMultiplexer connectionMultiplexer)
        {
            this.multiplexers.Add(connectionMultiplexer);
            this.redLockRetryConfiguration = new RedLockRetryConfiguration();
        }

        public LockFactory(ConnectionMultiplexer connectionMultiplexer, RedLockRetryConfiguration redLockRetryConfiguration)
        {
            this.multiplexers.Add(connectionMultiplexer);
            this.redLockRetryConfiguration = redLockRetryConfiguration;
        }

        public LockFactory(List<ConnectionMultiplexer> connectionMultiplexer)
        {
            foreach (var item in connectionMultiplexer)
            {
                this.multiplexers.Add(item);
            }
            this.redLockRetryConfiguration = new RedLockRetryConfiguration();
        }

        public LockFactory(List<ConnectionMultiplexer> connectionMultiplexer, RedLockRetryConfiguration redLockRetryConfiguration)
        {
            foreach (var item in connectionMultiplexer)
            {
                this.multiplexers.Add(item);
            }
            this.redLockRetryConfiguration = 
            this.redLockRetryConfiguration = redLockRetryConfiguration;
        }

        public void AddConnection(ConnectionMultiplexer connectionMultiplexer)
        {
            this.multiplexers.Add(connectionMultiplexer);
        }

        public void AddConnection(List<ConnectionMultiplexer> connectionMultiplexer)
        {
            foreach (var item in connectionMultiplexer)
            {
                this.multiplexers.Add(item);
            }
        }

        public RedLockFactory GetRedLock()
        {
            return RedLockFactory.Create(this.multiplexers, redLockRetryConfiguration);
        }

        /// <summary>
        /// Do job with Redlock
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        public T? DoJobWithRedLock<T>(Func<T> job)
        {
            T? result = default(T);
            var key = $"{Environment.MachineName}-{Process.GetCurrentProcess().Id}";
            var expiry = TimeSpan.FromSeconds(30);

            using (var redLock = GetRedLock().CreateLockAsync(key, expiry).Result)
            {
                if (redLock.IsAcquired)
                {
                    result = job.Invoke();
                }
            }

            return result;
        }

        /// <summary>
        /// Do job with Redislock
        /// </summary>
        /// <param name="key"></param>
        /// <param name="job"></param>
        /// <param name="lockExpireTime"></param>
        /// <param name="database"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public T? DoJobWithRedisLock<T>(string key, Func<T> job, TimeSpan lockExpireTime, IDatabase database, CommandFlags flags = CommandFlags.None)
        {
            T? result = default(T);
            RedisValue token = $"{Environment.MachineName}-{Process.GetCurrentProcess().Id}";

            if (database.LockTake(key, token, lockExpireTime, flags))
            {
                try
                {
                    result = job.Invoke();
                }
                finally
                {
                    database.LockRelease(key, token);
                }
            }

            return result;
        }
    }
}
