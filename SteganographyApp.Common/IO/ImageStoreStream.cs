namespace SteganographyApp.Common.IO;

using SteganographyApp.Common.Data;

/// <summary>
/// A wrapper class that exposes the IO related methods of an ImageStore instance while implementing
/// the IDisposable interface to safely close out any images loaded by the ImageStore while performing
/// more error prone IO operations.
/// </summary>
public sealed class ImageStoreStream : AbstractDisposable
{
    private readonly ImageStore store;

    private bool save = false;

    /// <summary>
    /// Initialize the wrapper using the image store that the IO method calls will be proxied to.
    /// </summary>
    /// <param name="store">The image store instance to be wrapped.</param>
    public ImageStoreStream(ImageStore store)
    {
        this.store = store;
        store.SeekToImage(0);
    }

    /// <summary>
    /// Invokes the wrapped image store WriteBinaryString method passing in the provided <paramref name="binary"/> argument.
    /// </summary>
    /// <param name="binary">The binary chunk to write to the cover images.</param>
    /// <see cref="ImageStore.WriteBinaryString(string)"/>
    /// <returns>A count of the number of bits that were written to the image.</returns>
    public int WriteContentChunkToImage(string binary) => RunIfNotDisposedWithResult(() =>
    {
        save = true;
        return store.WriteBinaryString(binary);
    });

    /// <summary>
    /// Invokes the wrapped image store ReadBinaryString methods passing in the provided <paramref name="length"/> argument.
    /// </summary>
    /// <param name="length">The number of bits to read from the cover images.</param>
    /// <returns>A binary string read from the cover images whose length is,
    /// at most, the length as specified by the input argument of the same name.</returns>
    public string ReadContentChunkFromImage(int length) => RunIfNotDisposedWithResult(() => store.ReadBinaryString(length));

    /// <summary>
    /// Invokes the wrapped image store SeekToPixel method passing in the provided <paramref name="bitsToSkip"/> argument.
    /// This will first move to the first pixel in the currently loaded image before skipping to the specified pixel.
    /// </summary>
    /// <param name="bitsToSkip">The number of bigs to seek past.</param>
    public void SeekToPixel(int bitsToSkip) => RunIfNotDisposed(() => store.SeekToPixel(bitsToSkip));

    /// <summary>
    /// Invokes the wrapped image store ResetToImage method passing in the provided <paramref name="index"/> argument.
    /// </summary>
    /// <param name="index">The index of the cover image to start reading and writing from.</param>
    public void SeekToImage(int index) => RunIfNotDisposed(() => store.SeekToImage(index));

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
        store.CloseOpenImage(save);
        store.StreamClosed();
    });
}