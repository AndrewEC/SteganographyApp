using System;

namespace SteganographyApp
{

    /// <summary>
    /// Thread-safe method to hold onto an error object so it can be shared
    /// between threads.
    /// </summary>
    class DecodeError
    {

        private readonly object _lock = new object();

        private Exception exception;

        public bool HasException()
        {
            lock (_lock)
            {
                return exception != null;
            }
        }

        public void PutException(Exception exception)
        {
            lock (_lock)
            {
                this.exception = exception;
            }
        }

        public Exception TakeException()
        {
            lock (_lock)
            {
                return exception;
            }
        }

    }

}