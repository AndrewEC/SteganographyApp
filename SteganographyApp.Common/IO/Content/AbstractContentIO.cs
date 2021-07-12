namespace SteganographyApp.Common.IO
{
    using System;

    using SteganographyApp.Common.Arguments;
    using SteganographyApp.Common.Injection;

    /// <summary>
    /// Base class for read and write IO. Implements the basic disposable logic to be shared
    /// with the content read and write child classes.
    /// </summary>
    public abstract class AbstractContentIO : IDisposable
    {
        /// <summary>
        /// Initialize the abstract content IO instance with the user provided input arguments.
        /// </summary>
        /// <param name="args">The user provided input arguments.</param>
        public AbstractContentIO(IInputArguments args)
        {
            this.Args = args;
        }

        /// <summary>
        /// Gets the values parsed from the command line arguments.
        /// </summary>
        protected IInputArguments Args { get; }

        /// <summary>
        /// Gets or sets the stream used by the underlying implementation to read
        /// or write data to a specified file.
        /// </summary>
        protected IReadWriteStream Stream { get; set; }

        /// <summary>
        /// Flushes the stream if it has been instantiated.
        /// </summary>
        public void Dispose()
        {
            if (Stream != null)
            {
                Stream.Flush();
                Stream.Dispose();
            }
        }
    }
}