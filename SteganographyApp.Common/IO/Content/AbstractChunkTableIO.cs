namespace SteganographyApp.Common.IO;

/// <summary>
/// The abstract base class for dealing with the content chunk table.
/// </summary>
/// <remarks>
/// Initializes the base class. This will keep a reference to the input arguments and
/// store a reference to the result of store.CreateIOWrapper for future use.
/// </remarks>
/// <param name="store">The image store instance.</param>
/// <param name="arguments">The user provided arguments.</param>
public abstract class AbstractChunkTableIO(ImageStore store, IInputArguments arguments)
{
    /// <summary>The number of iterations to be used when randomizing or reordering the chunk table binary.</summary>
    protected const int IterationMultiplier = 1000;

    /// <summary>The number of dummies to be inserted.</summary>
    protected const int DummyCount = 0;

    /// <summary>
    /// Gets the IO class for reading and writing binary data to/from the cover images.
    /// </summary>
    protected ImageStoreStream ImageStoreStream { get; } = store.OpenStream();

    /// <summary>
    /// Gets the user provided arguments from which we will pull the random seed to determine if
    /// the chunk table needs to be randomized or not.
    /// </summary>
    protected IInputArguments Arguments { get; } = arguments;
}