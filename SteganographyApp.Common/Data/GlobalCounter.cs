namespace SteganographyApp.Common.Data
{
    /// <summary>
    /// Keeps track of an arbitrary counter used when initializing a random generator
    /// instance as to increase the degree of variance among the positions and
    /// values of the dummy entries to increase the difficulty in discerning a pattern.
    /// </summary>
    public class GlobalCounter
    {
        public static readonly GlobalCounter Instance = new GlobalCounter();

        private static object syncLock = new object();

        private long count = 0;

        private GlobalCounter() { }

        public long Count
        {
            get
            {
                lock (syncLock)
                {
                    return count;
                }
            }

            private set
            {
                count = value;
            }
        }

        public void Increment(long amount)
        {
            lock (syncLock)
            {
                Count = Count + amount;
            }
        }

        public void Reset()
        {
            lock (syncLock)
            {
                Count = 0;
            }
        }
    }
}