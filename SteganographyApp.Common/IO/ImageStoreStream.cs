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
    /// <include file='./docs.xml' path='docs/members[@name="ImageStoreStream"]/WriteContentChunkToImage/*' />
    int WriteContentChunkToImage(string binary);

    /// <include file='./docs.xml' path='docs/members[@name="ImageStoreStream"]/ReadContentChunkFromImage/*' />
    string ReadContentChunkFromImage(int length);

    /// <include file='./docs.xml' path='docs/members[@name="ImageStoreStream"]/SeekToPixel/*' />
    void SeekToPixel(int bitsToSkip);

    /// <include file='./docs.xml' path='docs/members[@name="ImageStoreStream"]/SeekToImage/*' />
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

    /// <include file='./docs.xml' path='docs/members[@name="ImageStoreStream"]/WriteContentChunkToImage/*' />
    public int WriteContentChunkToImage(string binary) => RunIfNotDisposedWithResult(() =>
    {
        if (mode != StreamMode.Write)
        {
            throw new ImageStoreException("Stream cannot be used for writing as it was opened with the Read StreamMode.");
        }

        save = true;
        return store.WriteBinaryString(binary);
    });

    /// <include file='./docs.xml' path='docs/members[@name="ImageStoreStream"]/ReadContentChunkFromImage/*' />
    public string ReadContentChunkFromImage(int length) => RunIfNotDisposedWithResult(() =>
    {
        if (mode != StreamMode.Read)
        {
            throw new ImageStoreException("Stream cannot be used for reading as it was opened with the Write StreamMode.");
        }

        return store.ReadBinaryString(length);
    });

    /// <include file='./docs.xml' path='docs/members[@name="ImageStoreStream"]/SeekToPixel/*' />
    public void SeekToPixel(int bitsToSkip) => RunIfNotDisposed(() => store.SeekToPixel(bitsToSkip));

    /// <include file='./docs.xml' path='docs/members[@name="ImageStoreStream"]/SeekToImage/*' />
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