namespace SteganographyApp.Common.IO
{
    using System;

    using SteganographyApp.Common.Arguments;
    using SteganographyApp.Common.Injection;

    public abstract class AbstractContentIO : IDisposable
    {
        public AbstractContentIO(IInputArguments args)
        {
            this.Args = args;
        }

        /// <summary>
        /// The values parsed from the command line arguments.
        /// </summary>
        protected IInputArguments Args { get; }

        /// <summary>
        /// The stream used by the underlying implementation to read
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