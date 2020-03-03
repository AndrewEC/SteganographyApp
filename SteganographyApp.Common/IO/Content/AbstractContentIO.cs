using System;
using System.IO;

using SteganographyApp.Common.Arguments;

namespace SteganographyApp.Common.IO.Content
{
    public abstract class AbstractContentIO : IDisposable
    {

        /// <summary>
        /// The stream used by the underlying implementation to read
        /// or write data to a specified file.
        /// </summary>
        protected FileStream stream;

        /// <summary>
        /// The values parsed from the command line arguments.
        /// </summary>
        protected readonly IInputArguments args;

        public AbstractContentIO(IInputArguments args)
        {
            this.args = args;
        }

        /// <summary>
        /// Flushes the stream if it has been instantiated.
        /// </summary>
        public void Dispose()
        {
            if(stream != null)
            {
                stream.Flush();
                stream.Dispose();
            }
        }

    }
}
