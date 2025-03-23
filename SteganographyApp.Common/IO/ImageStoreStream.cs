namespace SteganographyApp.Common.IO;

using System;

#pragma warning disable SA1402

/// <summary>
/// A disposable wrapper for accessing the underlying <see cref="ImageStore"/> methods
/// for reading and writing data to the cover images.
/// </summary>
public interface IImageStoreStream : IDisposable
{
    /// <summary>
    /// The the binary chunk to the cover images.
    /// </summary>
    /// <param name="binary">The binary chunk to write to the cover images.</param>
    /// <returns>A count of the number of bits that were written to the image.</returns>
    int WriteContentChunkToImage(string binary);

    /// <summary>
    /// Reads a binary chunk of the specified length from the cover images.
    /// </summary>
    /// <param name="length">The number of bits to read from the cover images.</param>
    /// <returns>A binary string read from the cover images whose length is, at most,
    /// the length as specified by the input argument of the same name.</returns>
    string ReadContentChunkFromImage(int length);

    /// <summary>
    /// Skip over N pixels in the available cover images where N is either equal to the
    /// bit count / 2 or bit count / 3 dependending on the user provided arguments.
    /// </summary>
    /// <param name="bitsToSkip">The number of bits to seek past.</param>
    void SeekToPixel(int bitsToSkip);

    /// <summary>
    /// Loads the cover image available at the specified index.
    /// </summary>
    /// <param name="index">The index of the cover image to load.</param>
    void SeekToImage(int index);
}

/// <inheritdoc/>
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
    public void SeekToPixel(int bitsToSkip)
        => RunIfNotDisposed(() => store.SeekToPixel(bitsToSkip));

    /// <inheritdoc/>
    public void SeekToImage(int index)
        => RunIfNotDisposed(() => store.SeekToImage(index));

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