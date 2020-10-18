using System;

namespace SteganographyApp
{

    /// <summary>
    /// Thread-safe method to hold onto an error object so it can be shared
    /// between threads.
    /// </summary>
    class ErrorContainer
    {

        private readonly object _lock = new object();

        private Exception exception;

        /// <summary>
        /// Checks to see if the exception object is not null.
        /// </summary>
        public bool HasException()
        {
            lock (_lock)
            {
                return exception != null;
            }
        }

        /// <summary>
        /// Sets the exception reference so it can be read by the <see cref="Decoder" />.
        /// </summary>
        public void PutException(Exception exception)
        {
            lock (_lock)
            {
                if (this.exception != null)
                {
                    return;
                }
                this.exception = exception;
            }
        }

        /// <summary>
        /// Retrieves the value of the exception. Does not take the exception out
        /// in the sense that it will the reference to null.
        /// </summary>
        public Exception TakeException()
        {
            lock (_lock)
            {
                return exception;
            }
        }

    }

}