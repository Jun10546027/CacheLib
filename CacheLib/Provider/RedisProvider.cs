using CacheLib.Enum;
using CacheLib.Factory;
using CacheLib.Model;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace CacheLib.Provider
{
    public class RedisProvider : ICacheProvider
    {
        /// <summary>
        /// Redis Database
        /// </summary>
        private IDatabase _database { get; set; }

        /// <summary>
        /// Redis Server
        /// </summary>
        private IServer? _server { get; set; }

        /// <summary>
        /// Expire Time
        /// </summary>
        private TimeSpan _defaultExpireTime { get; set; }

        /// <summary>
        /// Lock Expire Time
        /// </summary>
        private TimeSpan _defaultLockExpireTime { get; set; }

        /// <summary>
        /// Which way to implement lock.
        /// </summary>
        private LockWayEnum _lockWay { get; set; }

        /// <summary>
        /// ILockFactory
        /// </summary>
        private ILockFactory _lockFactory { get; set; }

        /// <summary>
        /// Init RedisProvider by specify database 
        /// </summary>
        /// <param name="database">database</param>
        /// <param name="lockFactory">lock factory</param>
        /// <param name="defalutExpireTime">default expire time</param>
        /// <param name="defalutLockExpireTime">default lock expire time</param>
        /// <param name="lockWay">which lock implementation you want</param>
        public RedisProvider(IDatabase database, ILockFactory lockFactory, int defalutExpireTime = 86400, int defalutLockExpireTime = 1800, LockWayEnum lockWay = LockWayEnum.None)
        {
            this._server = null;
            this._database = database;
            this._defaultExpireTime = TimeSpan.FromSeconds(defalutExpireTime);
            this._defaultLockExpireTime = TimeSpan.FromSeconds(defalutLockExpireTime);
            this._lockWay = lockWay;
            this._lockFactory = lockFactory;
        }

        /// <summary>
        /// Init RedisProvider by specify server, database 
        /// </summary>
        /// <param name="server"></param>
        /// <param name="database"></param>
        /// <param name="defalutExpireTime"></param>
        public RedisProvider(IServer server, IDatabase database, ILockFactory lockFactory, int defalutExpireTime = 86400, int defalutLockExpireTime = 1800, LockWayEnum lockWay = LockWayEnum.RedisLock)
        {
            this._server = server;
            this._database = database;
            this._defaultExpireTime = TimeSpan.FromSeconds(defalutExpireTime);
            this._defaultLockExpireTime = TimeSpan.FromSeconds(defalutLockExpireTime);
            this._lockWay = lockWay;
            this._lockFactory = lockFactory;
        }

        /// <summary>
        /// Check key exist or not
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool IsExist(string key)
        {
            return this._database.KeyExists(key);
        }

        /// <summary>
        /// Get string data via key. If data doesn't exist the special value nil will be returned.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public string? Get(string key, CommandFlags flags = CommandFlags.None)
        {
            return this._database.StringGet(key, flags);
        }

        /// <summary>
        /// Get string data via key. If data doesn't exist the special value nil will be returned.
        /// And refresh expire time.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="expireTime"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public string? Get(string key, TimeSpan expireTime, CommandFlags flags = CommandFlags.None)
        {
            return this._database.StringGetSetExpiry(key, expireTime, flags);
        }

        /// <summary>
        /// Get data via key. If data doesn't exist the special value nil will be returned.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public T? Get<T>(string key, CommandFlags flags = CommandFlags.None) where T : class
        {
            string? value = this.Get(key, flags);

            if (string.IsNullOrWhiteSpace(value))
            {
                return default(T);
            }

            return JsonConvert.DeserializeObject<T>(value);
        }

        /// <summary>
        /// Get data via key. If data doesn't exist the special value nil will be returned.
        /// And refresh expire time.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public T? Get<T>(string key, TimeSpan expireTime, CommandFlags flags = CommandFlags.None) where T : class
        {
            string? value = this.Get(key, expireTime, flags);

            if (string.IsNullOrWhiteSpace(value))
            {
                return default(T);
            }

            return JsonConvert.DeserializeObject<T>(value);
        }

        /// <summary>
        /// Get data and expire time via key. 
        /// If data doesn't exist the special value nil will be returned.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public CacheEntity<T>? GetWithExpireTime<T>(string key, CommandFlags flags = CommandFlags.None) where T : class
        {
            var result = this._database.StringGetWithExpiry(key, flags);

            if (result.Value.IsNullOrEmpty)
            {
                return default(CacheEntity<T>);
            }

            return new CacheEntity<T>()
            {
                Data = JsonConvert.DeserializeObject<T>(result.Value),
                Expire = result.Expiry
            };
        }

        /// <summary>
        /// Get string data via key. If data doesn't exist the special value nil will be returned.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public async Task<string?> GetAsync(string key, CommandFlags flags = CommandFlags.None)
        {
            return await this._database.StringGetAsync(key, flags);
        }

        /// <summary>
        /// Get data via key. If data doesn't exist the special value nil will be returned.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public async Task<T?> GetAsync<T>(string key, CommandFlags flags = CommandFlags.None) where T : class
        {
            string? value = await this._database.StringGetAsync(key, flags);

            if (string.IsNullOrWhiteSpace(value))
            {
                return default(T);
            }

            return JsonConvert.DeserializeObject<T>(value);
        }

        /// <summary>
        /// Set string via key. True when the set successfully, otherwise false.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="flags"></param>
        /// <param name="expireTime"></param>
        /// <returns></returns>
        public bool Set(string key, string? value, CommandFlags flags = CommandFlags.None, TimeSpan? expireTime = null)
        {
            bool result = false;

            Func<bool> job = new Func<bool>(() => {
                var expire = expireTime ?? this._defaultExpireTime;
                return this._database.StringSet(key, value, expire);
            });

            switch (this._lockWay)
            {
                case LockWayEnum.RedLock:
                    result = this._lockFactory.DoJobWithRedLock<bool>(job);
                    break;
                case LockWayEnum.RedisLock:
                    Guid redisKey = Guid.NewGuid();
                    result =  this._lockFactory.DoJobWithRedisLock<bool>(redisKey.ToString(), job, this._defaultLockExpireTime, this._database);
                    break;
                default:
                    result = job();
                    break;
            }

            return result;
        }

        /// <summary>
        /// Set data via key. True when the set successfully, otherwise false.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="flags"></param>
        /// <param name="expireTime"></param>
        /// <returns></returns>
        public bool Set<T>(string key, T value, CommandFlags flags = CommandFlags.None, TimeSpan? expireTime = null) where T : class
        {
            return this.Set(key, JsonConvert.SerializeObject(value).ToString(), flags, expireTime);
        }

        /// <summary>
        /// Remove data via key
        /// </summary>
        /// <param name="key"></param>
        public void Remove(string key, CommandFlags flags = CommandFlags.None)
        {
            this._database.KeyDelete(key, flags);
        }

        /// <summary>
        /// Increase value via key. If have set expire time will be update on this key, otherwise keep current expire time.  
        /// </summary>
        /// <param name="key"></param>
        /// <param name="increaseVal"></param>
        /// <param name="flags"></param>
        /// <param name="expireTime"></param>
        /// <returns>Result</returns>
        public long IncrBy(string key, long increaseVal, CommandFlags flags = CommandFlags.None, TimeSpan? expireTime = null)
        {
            var result = this._database.StringIncrement(key, increaseVal, flags);

            if (expireTime.HasValue)
            {
                this._database.KeyExpire(key, expireTime, flags);
            }

            return result;
        }

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
        public List<string> GetKeyList(int db = -1, string pattern = "", int pageSize = 250, int cursor = 0, int offset = 0, CommandFlags flags = CommandFlags.None)
        {
            var result = new List<string>();

            if (this._server is null)
            {
                return result;
            }

            var keyList = this._server.Keys(db, pattern, pageSize, cursor, offset, flags);

            if (keyList is null || keyList.Any() == false)
            {
                return result;
            }

            result = keyList.Select(x => x.ToString()).ToList();
            return result;
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
            var result = new List<string?>();
            var skipped = 0;
            IEnumerable<string> batchValue;

            while ((batchValue = cacheKeys.Skip(skipped).Take(batchSize)).Any())
            {
                var batch = this._database.CreateBatch();
                var tasks = batchValue.Select(x => batch.StringGetAsync(x)).ToArray();

                batch.Execute();
                Task.WhenAll(tasks);

                foreach (var targetTask in tasks)
                {
                    if (targetTask.Result.IsNullOrEmpty == true)
                    {
                        result.Add(default(string));
                    }
                    else
                    {
                        result.Add(JsonConvert.DeserializeObject<string>(targetTask.Result.ToString()));
                    }
                }

                skipped += batchSize;
            }

            return result;
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
            var result = new List<T?>();
            var skipped = 0;
            IEnumerable<string> batchValue;

            while ((batchValue = cacheKeys.Skip(skipped).Take(batchSize)).Any())
            {
                var batch = this._database.CreateBatch();
                var tasks = batchValue.Select(x => batch.StringGetAsync(x)).ToArray();

                batch.Execute();
                Task.WhenAll(tasks);

                foreach (var targetTask in tasks)
                {
                    if (targetTask.Result.IsNullOrEmpty == true)
                    {
                        result.Add(default(T));
                    }
                    else
                    {
                        string? tmpValue = targetTask.Result.ToString();
                        result.Add(JsonConvert.DeserializeObject<T>(tmpValue));
                    }
                }

                skipped += batchSize;
            }

            return result;
        }

        /// <summary>
        /// Set data by batch.
        /// If success will return -1, otherwise return the index of fail KeyValuePair in input collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKeysValue"></param>
        /// <param name="batchSize"></param>
        /// <returns></returns>
        public int StringBatchSet<T>(List<KeyValuePair<string, T>> cacheKeysValue, int batchSize = 10, TimeSpan? expireTime = null) where T : class
        {
            var result = 0;

            Func<Int32> job = new Func<Int32>(() => {
                var Idx = 0;
                var skipped = 0;
                IEnumerable<KeyValuePair<string, T>> batchedValues;

                if (expireTime.HasValue == false)
                {
                    expireTime = this._defaultExpireTime;
                }

                while ((batchedValues = cacheKeysValue.Skip(skipped).Take(batchSize)).Any())
                {
                    var batch = this._database.CreateBatch();
                    var tasks = batchedValues.Select(
                                    x => batch.StringSetAsync(x.Key, JsonConvert.SerializeObject(x.Value), expireTime)
                                ).ToArray();

                    batch.Execute();
                    Task.WhenAll(tasks);

                    foreach (var task in tasks)
                    {
                        if (task.Result == false) return Idx;
                        Idx++;
                    }

                    skipped += batchSize;
                }

                return -1;
            });

            switch (this._lockWay)
            {
                case LockWayEnum.RedLock:
                    result = this._lockFactory.DoJobWithRedLock<int>(job);
                    break;
                case LockWayEnum.RedisLock:
                    Guid redisKey = Guid.NewGuid();
                    result = this._lockFactory.DoJobWithRedisLock<int>(redisKey.ToString(), job, this._defaultLockExpireTime, this._database);
                    break;
                default:
                    result = job();
                    break;
            }

            return result;
        }

        /// <summary>
        /// Use transation scope when set variable asynchronously
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="flags"></param>
        /// <param name="expireTime"></param>
        /// <returns></returns>
        public async Task<bool> SetByTransationAsync(string key, string? value, CommandFlags flags = CommandFlags.None, TimeSpan? expireTime = null)
        {
            bool result = false;

            Func<Task<bool>> job = new Func<Task<bool>>(async () => {
                
                var tran = this._database.CreateTransaction();

                var expire = expireTime ?? this._defaultExpireTime;
                var stringSetTask = tran.StringSetAsync(key, value);
                tran.AddCondition(Condition.KeyNotExists(key));
                bool committed = await tran.ExecuteAsync();

                if (committed)
                {
                    await stringSetTask;
                    return true;
                }
                else
                {
                    return false;
                }
            });

            switch (this._lockWay)
            {
                case LockWayEnum.RedLock:
                    result = await this._lockFactory.DoJobWithRedLockAsync<bool>(job);
                    break;
                case LockWayEnum.RedisLock:
                    Guid redisKey = Guid.NewGuid();
                    result = await this._lockFactory.DoJobWithRedisLockAsync<bool>(redisKey.ToString(), job, this._defaultLockExpireTime, this._database);
                    break;
                default:
                    result = await job.Invoke();
                    break;
            }

            return result;
        }

        /// <summary>
        /// Use transation scope when set variable asynchronously
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="flags"></param>
        /// <param name="expireTime"></param>
        /// <returns></returns>
        public async Task<bool> SetByTransationAsync<T>(string key, T? value, CommandFlags flags = CommandFlags.None, TimeSpan? expireTime = null) where T : class
        {
            return await this.SetByTransationAsync(key, JsonConvert.SerializeObject(value), flags, expireTime);
        }

        /// <summary>
        /// Run the lua script and no return.
        /// </summary>
        /// <param name="luaScript"></param>
        /// <returns></returns>
        public async Task RunWithLua(string luaScript)
        {
            await this._database.ScriptEvaluateAsync(luaScript);
        }

        /// <summary>
        /// Run the lua script and return result.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="luaScript"></param>
        /// <returns></returns>
        public async Task<T?> RunWithLua<T>(string luaScript)
        {
            var result = await this._database.ScriptEvaluateAsync(luaScript);
            string? value = result.ToString();

            if (string.IsNullOrWhiteSpace(value))
            {
                return default(T);
            }

            return JsonConvert.DeserializeObject<T>(value);
        }
    }
}