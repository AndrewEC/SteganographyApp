namespace SteganographyApp.Common.Data
{
    /// <summary>
    /// Keeps track of an arbitrary counter used when initializing a random generator
    /// instance as to increase the degree of variance among the positions and
    /// values of the dummy entries to increase the difficulty in discerning a pattern.
    /// </summary>
    public sealed class GlobalCounter
    {
        /// <summary>
        /// The singleton instance of the GlobalCounter.
        /// </summary>
        public static readonly GlobalCounter Instance = new GlobalCounter();

        private static readonly object SyncLock = new object();

        private long count = 0;
        private long lastCount = 0;

        private GlobalCounter() { }

        /// <summary>
        /// Gets the current accumulated count.
        /// </summary>
        public long Count
        {
            get
            {
                lock (SyncLock)
                {
                    return count;
                }
            }

            private set
            {
                lastCount = count;
                count = value;
            }
        }

        /// <summary>
        /// Gets the previous accumulated count.
        /// </summary>
        public long LastCount
        {
            get
            {
                lock (SyncLock)
                {
                    return lastCount;
                }
            }
        }

        /// <summary>
        /// Attempts to increment the Count value by the specified amount.
        /// </summary>
        /// <param name="amount">The amount to increment Count by.</param>
        public void Increment(long amount)
        {
            lock (SyncLock)
            {
                Count = Count + amount;
            }
        }

        /// <summary>
        /// Resets the count value to 0.
        /// </summary>
        public void Reset()
        {
            lock (SyncLock)
            {
                Count = 0;
            }
        }
    }
}