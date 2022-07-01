namespace CacheLib.Enum
{
    public enum LockWayEnum
    {
        /// <summary>
        /// No implement lock
        /// </summary>
        None,

        /// <summary>
        /// Implement lock by redis LockTake and LockRelease
        /// </summary>
        RedisLock,

        /// <summary>
        /// Implement lock by RedLock
        /// </summary>
        RedLock
    }
}
