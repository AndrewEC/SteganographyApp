namespace SteganographyApp.Common.IO;

using SteganographyApp.Common.Arguments;

/// <summary>
/// The abstract base class for dealing with the content chunk table.
/// </summary>
/// <remarks>
/// Initializes the base class. This will keep a reference to the input arguments and
/// store a reference to the result of store.CreateIOWrapper for future use.
/// </remarks>
/// <param name="store">The image store instance.</param>
/// <param name="arguments">The user provided arguments.</param>
public abstract class AbstractChunkTableIO(ImageStore store, IInputArguments arguments) : AbstractDisposable
{
    /// <summary>The number of iterations to be used when randomizing or reordering the chunk table binary.</summary>
    protected const int IterationMultiplier = 1000;

    /// <summary>The number of dummies to be inserted.</summary>
    protected const int DummyCount = 0;

    /// <summary>
    /// Gets the IO class for reading and writing binary data to/from the cover images.
    /// </summary>
    protected ImageStoreIO ImageStoreIO { get; } = store.CreateIOWrapper();

    /// <summary>
    /// Gets the user provided arguments from which we will pull the random seed to determine if
    /// the chunk table needs to be randomized or not.
    /// </summary>
    protected IInputArguments Arguments { get; } = arguments;

    /// <summary>
    /// Disposes of the current instance. Any implementation of this method should check if disposing is true and,
    /// if it is not, skip the execution of the remainder of the method.
    /// </summary>
    /// <param name="disposing">Indicates if this method was called from the base Dispose method.</param>
    protected override void Dispose(bool disposing) => RunIfNotDisposed(() =>
    {
        if (!disposing)
        {
            return;
        }
        ImageStoreIO.Dispose();
    });
}