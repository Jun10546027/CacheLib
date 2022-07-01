using StackExchange.Redis;

namespace CacheLib
{
    public sealed class RedisConnManager
    {
        private readonly ConfigurationOptions? _option;
        private static Lazy<ConnectionMultiplexer> _connection;
        private static readonly object _locker = new object();

        /// <summary>
        /// Implement ConnectionMultiplexer Factory by Singleton pattern 
        /// </summary>
        /// <param name="options"></param>
        public RedisConnManager(ConfigurationOptions options)
        {
            this._option = options;
            lock (_locker)
            {
                if (_connection == null)
                {
                    _connection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(this._option));
                }
            }
        }

        /// <summary>
        /// Implement ConnectionMultiplexer Factory by Singleton pattern 
        /// </summary>
        /// <param name="connectionStr"></param>
        public RedisConnManager(string connectionStr)
        {
            lock (_locker)
            {
                if (_connection == null)
                {
                    _connection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(connectionStr));
                }
            }
        }

        /// <summary>
        /// Get all connection in ConnectionMultiplexer.
        /// </summary>
        /// <returns></returns>
        public ConnectionMultiplexer GetConnection() => _connection.Value;

        /// <summary>
        /// Get all redis db in ConnectionMultiplexer.
        /// </summary>
        /// <returns></returns>
        public IDatabase GetRedisDB() => GetConnection().GetDatabase();

        /// <summary>
        /// Get specify redis db by index in ConnectionMultiplexer.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public IDatabase GetRedisDB(int index) => GetConnection().GetDatabase(index);
    }
}
