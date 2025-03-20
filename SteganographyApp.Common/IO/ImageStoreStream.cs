namespace SteganographyApp.Common.IO;

using System;

#pragma warning disable SA1402

/// <summary>
/// A wrapper that exposes the IO related methods of an ImageStore instance while implementing
/// the IDisposable interface to safely close out any images loaded by the ImageStore while performing
/// more error prone IO operations.
/// </summary>
public interface IImageStoreStream : IDisposable
{
    /// <summary>
    /// Invokes the wrapped image store WriteBinaryString method passing in the provided
    /// <paramref name="binary"/> argument.
    /// </summary>
    /// <param name="binary">The binary chunk to write to the cover images.</param>
    /// <returns>A count of the number of bits that were written to the image.</returns>
    int WriteContentChunkToImage(string binary);

    /// <summary>
    /// Invokes the wrapped image store ReadBinaryString methods passing in the provided <paramref name="length"/> argument.
    /// </summary>
    /// <param name="length">The number of bits to read from the cover images.</param>
    /// <returns>A binary string read from the cover images whose length is, at most,
    /// the length as specified by the input argument of the same name.</returns>
    string ReadContentChunkFromImage(int length);

    /// <summary>
    /// Invokes the wrapped image store SeekToPixel method passing in the provided <paramref name="bitsToSkip"/> argument.
    /// This will first move to the first pixel in the currently loaded image before skipping to the specified pixel.
    /// </summary>
    /// <param name="bitsToSkip">The number of bigs to seek past.</param>
    void SeekToPixel(int bitsToSkip);

    /// <summary>
    /// Invokes the wrapped image store ResetToImage method passing in the provided <paramref name="index"/> argument.
    /// </summary>
    /// <param name="index">The index of the cover image to start reading and writing from.</param>
    void SeekToImage(int index);
}

/// <summary>
/// A wrapper class that exposes the IO related methods of an ImageStore instance while implementing
/// the IDisposable interface to safely close out any images loaded by the ImageStore while performing
/// more error prone IO operations.
/// </summary>
public sealed class ImageStoreStream : AbstractDisposable, IImageStoreStream
{
    private readonly ImageStore store;
    private readonly StreamMode mode;
    private bool save = false;

    /// <summary>
    /// Initialize the wrapper using the image store that the IO method calls will be proxied to.
    /// </summary>
    /// <param name="store">The image store instance to be wrapped.</param>
    /// <param name="mode">The mode specifying which types of IO operations should
    /// be permitted.</param>
    public ImageStoreStream(ImageStore store, StreamMode mode)
    {
        this.store = store;
        this.mode = mode;
        store.SeekToImage(0);
    }

    /// <inheritdoc/>
    public int WriteContentChunkToImage(string binary) => RunIfNotDisposedWithResult(() =>
    {
        if (mode != StreamMode.Write)
        {
            throw new ImageStoreException("Stream cannot be used for writing as it was opened with the Read StreamMode.");
        }

        save = true;
        return store.WriteBinaryString(binary);
    });

    /// <inheritdoc/>
    public string ReadContentChunkFromImage(int length) => RunIfNotDisposedWithResult(() =>
    {
        if (mode != StreamMode.Read)
        {
            throw new ImageStoreException("Stream cannot be used for reading as it was opened with the Write StreamMode.");
        }

        return store.ReadBinaryString(length);
    });

    /// <inheritdoc/>
    public void SeekToPixel(int bitsToSkip) => RunIfNotDisposed(() => store.SeekToPixel(bitsToSkip));

    /// <inheritdoc/>
    public void SeekToImage(int index) => RunIfNotDisposed(() => store.SeekToImage(index));

    /// <summary>
    /// Disposes of the current instance. Any implementation of this method should check if disposing is true and,
    /// if it is not, skip the execution of the remainder of the method.
    /// </summary>
    protected override void DoDispose() => RunIfNotDisposed(() =>
    {
        store.CloseOpenImage(save);
        store.StreamClosed();
    });
}

#pragma warning restore SA1402