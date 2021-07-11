namespace SteganographyApp.Common.IO
{
    using System;

    public sealed partial class ImageStore
    {
        /// <summary>
        /// A wrapper class that exposes the IO related methods of an ImageStore instance while implementing
        /// the IDisposable interface to safely close out any images loaded by the ImageStore while performing
        /// more error prone IO operations.
        /// </summary>
        public class ImageStoreWrapper : IDisposable
        {
            private readonly ImageStore store;

            private bool save = false;

            public ImageStoreWrapper(ImageStore store)
            {
                this.store = store;
                store.LoadNextImage();
            }

            public void EncodeComplete()
            {
                save = true;
            }

            public int WriteContentChunkToImage(string binary) => store.WriteBinaryString(binary);

            public string ReadContentChunkFromImage(int length) => store.ReadBinaryString(length);

            public void SeekToPixel(int bitsToSkip) => store.SeekToPixel(bitsToSkip);

            public void ResetToImage(int index) => store.ResetToImage(index);

            public void Dispose()
            {
                store.CloseOpenImage(save);
                store.ResetToImage(0);
            }
        }
    }
}