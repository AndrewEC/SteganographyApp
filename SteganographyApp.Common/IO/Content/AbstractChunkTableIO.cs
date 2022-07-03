namespace SteganographyApp.Common.IO
{
    using System;

    using SteganographyApp.Common.Arguments;

    /// <summary>
    /// The abstract base class for dealing with the content chunk table.
    /// </summary>
    public abstract class AbstractChunkTableIO : IDisposable
    {
        /// <summary>
        /// Initializes the base class. This will keep a reference to the input arguments and
        /// store a reference to the result of store.CreateIOWrapper for future use.
        /// </summary>
        /// <param name="store">The image store instance.</param>
        /// <param name="arguments">The user provided arguments.</param>
        public AbstractChunkTableIO(ImageStore store, IInputArguments arguments)
        {
            ImageStoreIO = store.CreateIOWrapper();
            Arguments = arguments;
        }

        /// <summary>
        /// Gets the IO class for reading and writing binary data to/from the cover images.
        /// </summary>
        protected ImageStoreIO ImageStoreIO { get; }

        /// <summary>
        /// Gets the user provided arguments from which we will pull the random seed to determine if
        /// the chunk table needs to be randomized or not.
        /// </summary>
        protected IInputArguments Arguments { get; }

        /// <summary>
        /// Stores a reference to the image store IO wrapper created at the time this base class
        /// was initialized.
        /// </summary>
        public void Dispose()
        {
            ImageStoreIO.Dispose();
        }
    }
}