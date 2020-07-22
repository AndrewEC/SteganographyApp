using System;
using System.Linq;
using System.Text;

using SixLabors.ImageSharp.PixelFormats;

using SteganographyApp.Common.Arguments;
using SteganographyApp.Common.Providers;

namespace SteganographyApp.Common.IO
{

    /// <summary>
    /// Class that handles positioning a make shift write stream in the proper position so
    /// data can be reliably read and written to the storage images.
    /// </summary>
    public class ImageStore
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

            public int WriteContentChunkToImage(string binary)
            {
                return store.WriteBinaryString(binary);
            }

            public string ReadContentChunkFromImage(int length)
            {
                return store.ReadBinaryString(length);
            }

            public void SeekToPixel(int bitsToSkip)
            {
                store.SeekToPixel(bitsToSkip);
            }

            public void ResetToImage(int index)
            {
                store.ResetToImage(index);
            }

            public bool HasEnoughSpaceForContentChunkTable()
            {
                return store.HasEnoughSpaceForContentChunkTable();
            }

            public void Dispose()
            {
                store.CloseOpenImage(save);
                store.ResetToImage(0);
            }
        }

        /// <summary>
        /// Event handler that's invoked whenever the WriteContentChunkToImage method completes.
        /// The main argument of the event will indicate the total number of bits written to the
        /// image(s).
        /// </summary>
        public event EventHandler<ChunkWrittenArgs> OnChunkWritten;

        /// <summary>
        /// Event handler that's invoked whenever the internal Next method is called.
        /// In the Next method another image will be loaded so it can be read/written to.
        /// The path of the file will be the main argument passed to the event handler.
        /// </summary>
        public event EventHandler<NextImageLoadedEventArgs> OnNextImageLoaded;

        /// <summary>
        /// Stores the current x and y position for the current read/write operation.
        /// </summary>
        private readonly Position position = new Position();

        /// <summary>
        /// The index used to determine which image will be read/written to in the
        /// next read/write operation.
        /// <para>The image name is retrieved by looking up the args.CoverImage value
        /// using this field as the index.</para>
        /// </summary>
        private int currentImageIndex = -1;

        /// <summary>
        /// The currently loaded image. The image is loaded whenever the Next
        /// method is called and the currentImageIndex has been incremented.
        /// </summary>
        private IBasicImageInfo currentImage;

        /// <summary>
        /// The values parsed from the command line arguments.
        /// </summary>
        private readonly IInputArguments args;

        /// <summary>
        /// Readonly property that returns the name of the current image being used in the write
        /// process.
        /// </summary>
        private string CurrentImage
        {
            get
            {
                return args.CoverImages[currentImageIndex];
            }
        }

        /// <summary>
        /// Creates a new instance of the ImageStore and calculates the RequiredContentChunkTableBitSize
        /// value.
        /// </summary>
        /// <param name="args">The values parsed from the command line arguments.</param>
        public ImageStore(IInputArguments args)
        {
            this.args = args;
        }

        /// <summary>
        /// Utility method to create an <see cref="ImageStoreWrapper"/> instance to be used in conjunction with the
        /// read and write methods.
        /// <para>The ImageStoreWrapper instance provides proxies to the ImageStore IO methods as well as
        /// implementing the IDisposable interface to ensure that any currently open and loaded image
        /// will be properly disposed.</para>
        /// </summary>
        /// <returns>A new ImageStoreWrapper instance.</returns>
        public ImageStoreWrapper CreateIOWrapper()
        {
            return new ImageStoreWrapper(this);
        }

        /// <summary>
        /// Will look over all images specified in the InputArguments
        /// and set the LSB in all pixels in all images to 0.
        /// </summary>
        public void CleanImageLSBs()
        {
            try
            {
                ResetToImage(0);
                var randomBit = RandomBitGenerator();
                for (int i = 0; i < args.CoverImages.Length; i++)
                {
                    while (true)
                    {
                        byte newRed = (byte)(currentImage[position.X, position.Y].R & ~randomBit());
                        byte newGreen = (byte)(currentImage[position.X, position.Y].G & ~randomBit());
                        byte newBlue = (byte)(currentImage[position.X, position.Y].B & ~randomBit());

                        currentImage[position.X, position.Y] = new Rgba32(newRed, newGreen, newBlue, currentImage[position.X, position.Y].A);

                        if (!TryMoveToNextPixel())
                        {
                            break;
                        }
                    }
                    if (i == args.CoverImages.Length - 1)
                    {
                        CloseOpenImage(true);
                    }
                    else
                    {
                        LoadNextImage(true);
                    }
                }
            }
            catch (Exception e)
            {
                CloseOpenImage();
                throw e;
            }
        }

        /// <summary>
        /// Provides a consumable function that uses the standard Random class to generate
        /// a random int value of either 0 or 1.
        /// </summary>
        private Func<int> RandomBitGenerator()
        {
            var random = new Random();
            return () => (int) Math.Round(random.NextDouble());
        }

        /// <summary>
        /// Writes a binary string value to the current image, starting
        /// at the last write index, by replacing the LSB of each RGB value in each pixel.
        /// <para>If the current image does not have enough storage space to store the binary
        /// string in full then the Next method will be invoked and it will continue to write
        /// on the next available image.</para>
        /// </summary>
        /// <param name="binary">The encypted binary string to write to the image.</param>
        /// <returns>The number of bits written to the image. This is mostly important when
        /// writing the content chunk table to the start of the leading image.</returns>
        /// <exception cref="ImageProcessingException">Rethrown from the Next method call.</exception>
        private int WriteBinaryString(string binary)
        {
            int written = 0;
            for (int i = 0; i < binary.Length; i += 3)
            {
                Rgba32 pixel = currentImage[position.X, position.Y];

                pixel.R = (binary[i] == '0') ? (byte)(pixel.R & ~1) : (byte)(pixel.R | 1);
                written++;

                if (written < binary.Length)
                {
                    pixel.G = (binary[i + 1] == '0') ? (byte)(pixel.G & ~1) : (byte)(pixel.G | 1);
                    written++;

                    if (written < binary.Length)
                    {
                        pixel.B = (binary[i + 2] == '0') ? (byte)(pixel.B & ~1) : (byte)(pixel.B | 1);
                        written++;
                    }
                }

                currentImage[position.X, position.Y] = new Rgba32(
                        pixel.R,
                        pixel.G,
                        pixel.B,
                        pixel.A
                    );

                if (!TryMoveToNextPixel())
                {
                    LoadNextImage(true);
                }
            }
            OnChunkWritten?.Invoke(this, new ChunkWrittenArgs { ChunkLength = written });
            return written;
        }

        /// <summary>
        /// Attempts to read the specified number of bits from the current image starting
        /// at the last read/write position.
        /// <para>If there is not enough available space in the current image then it will
        /// invoke the Next method and continue to read from the next available image.</para>
        /// </summary>
        /// <param name="bitsToRead">Specifies the number of bits to be read from the current image.</param>
        /// <returns>A binary string whose length is equal to the length specified in the length
        /// parameter.</returns>
        /// <exception cref="ImageProcessingException">Rethrown from the Next method call.</exception>
        private string ReadBinaryString(int bitsToRead)
        {
            var binary = new StringBuilder();
            int bitsRead = 0;
            while (bitsRead < bitsToRead)
            {
                Rgba32 pixel = currentImage[position.X, position.Y];

                string redBit = Convert.ToString(pixel.R, 2);
                binary.Append(redBit.Substring(redBit.Length - 1));
                bitsRead++;

                if (bitsRead < bitsToRead)
                {
                    string greenBit = Convert.ToString(pixel.G, 2);
                    binary.Append(greenBit.Substring(greenBit.Length - 1));
                    bitsRead++;

                    if (bitsRead < bitsToRead)
                    {
                        string blueBit = Convert.ToString(pixel.B, 2);
                        binary.Append(blueBit.Substring(blueBit.Length - 1));
                        bitsRead++;
                    }
                }

                if (!TryMoveToNextPixel())
                {
                    LoadNextImage();
                }
            }
            return binary.ToString();
        }

        /// <summary>
        /// Reads the content chunk table from the current image and returns each
        /// content entry except for the first meta entry as a list of ints.
        /// <para>Each values in the list specifies the number of bits that make
        /// up a chunk of encrypted data that makes up the original encoded file.</para>
        /// </summary>
        /// <returns>A list of int32 values specifies the bit length of each encrypted chunk
        /// of data stored in the current set of images.</returns>
        /// <exception cref="ImageProcessingException">Thrown if images do not have enough
        /// storage space to read the entire content chunk table.</exception>
        public int[] ReadContentChunkTable()
        {
            try
            {
                // The size of each table entry is 32 bits but is made up of 11 pixels.
                // Each pixel can store 3 bits so we end up with one bit of padding on the
                // end. So we need to move in increments of 33 bits as we are reading sequential
                // entries in the chunk table.
                int chunkSizeAndPadding = Calculator.ChunkDefinitionBitSize + 1;

                // The first 32 bits of the table represent the number of chunk lengths
                // contained within the table.
                int chunkCount = Convert.ToInt32(ReadBinaryString(Calculator.ChunkDefinitionBitSize), 2);

                //Read all the available entries in the table
                int bitsForAllTableEntries = chunkCount * chunkSizeAndPadding;
                string tableEntriesBinary = ReadBinaryString(bitsForAllTableEntries);

                return Enumerable.Range(0, chunkCount)
                    .Select(index => Convert.ToInt32(tableEntriesBinary.Substring(index * chunkSizeAndPadding, Calculator.ChunkDefinitionBitSize), 2))
                    .ToArray();
            }
            catch (Exception e)
            {
                CloseOpenImage();
                throw e;
            }
        }

        /// <summary>
        /// Writes the content chunk table so that the proper number of bits can be read
        /// from the target images and properly decrypted.
        /// </summary>
        /// <param name="chunkTable">A list containing the bit length of each chunk of data that was
        /// originally written to the target images during the encoding process.</param>
        /// <exception cref="ImageProcessingException">Thrown if the leading image does not have enough
        /// storage space to store the entire content chunk table.</exception>
        public void WriteContentChunkTable(int[] chunkTable)
        {
            try
            {
                var binary = new StringBuilder("");

                // Each table entry is 32 bits in size meaning that since each pixel can store 3 bits it will take
                // 11 pixels. Since 11 pixels can actually store 33 bits we pad the 32 bit table entry with an additional
                // zero at the end which will be ignored when reading the table.
                binary.Append(Convert.ToString(chunkTable.Length, 2).PadLeft(Calculator.ChunkDefinitionBitSize, '0')).Append('0');

                foreach (int chunkLength in chunkTable)
                {
                    binary.Append(Convert.ToString(chunkLength, 2).PadLeft(Calculator.ChunkDefinitionBitSize, '0')).Append('0');
                }

                WriteBinaryString(binary.ToString());
                CloseOpenImage(true);
            }
            catch (Exception e)
            {
                CloseOpenImage();
                throw e;
            }
        }

        /// <summary>
        /// Checks to see if the current image has enough available storage space to store the entirety
        /// of the content chunk table.
        /// </summary>
        /// <returns>Returns true if the number of bits the image can store is more than the estimated number of
        /// bits required for the content chunk table.</returns>
        private bool HasEnoughSpaceForContentChunkTable()
        {
            int requiredBitsForContentTable = Calculator.CalculateRequiredBitsForContentTable(args.FileToEncode, args.ChunkByteSize);
            return (currentImage.Width * currentImage.Height * Calculator.BitsPerPixel) > requiredBitsForContentTable;
        }

        /// <summary>
        /// Moves the current index back to the index before the specified image
        /// then calls the Next method to advance to the specified image.
        /// </summary>
        /// <param name="imageName">The image name to start reading and writing from.</param>
        /// <exception cref="ImageProcessingException">Rethrown from the Next method call.</exception>
        public void ResetToImage(int coverImageIndex)
        {
            if (coverImageIndex < 0 || coverImageIndex >= args.CoverImages.Length)
            {
                throw new ImageProcessingException($"An invalid image index was provided in ResetTo. Expected value between {0} and {args.CoverImages.Length - 1}, instead got {coverImageIndex}");
            }
            currentImageIndex = coverImageIndex - 1;
            LoadNextImage();
        }

        /// <summary>
        /// Attempts to save then dispose of the currently opened image, if the currentImage property
        /// is not null, resets the read/write x and y positions to 0, increments the current image index,
        /// loads the new image, and invokes the OnNextImageLoaded event.
        /// </summary>
        /// <param name="saveImageChanges">If true it will attempt to save any changes made to the currently open
        /// image before attempting to increment to the next available image.</param>
        /// <exception cref="ImageProcessingException">If the increment of the current
        /// image index exeeds the available number of images an exception will be thrown.</exception>
        private void LoadNextImage(bool saveImageChanges = false)
        {
            CloseOpenImage(saveImageChanges);

            position.Reset();
            currentImageIndex++;
            if (currentImageIndex == args.CoverImages.Length)
            {
                throw new ImageProcessingException("There is not enough available storage space in the provided images to process this request.");
            }

            currentImage = Injector.Provide<IImageProvider>().LoadImage(args.CoverImages[currentImageIndex]);

            OnNextImageLoaded?.Invoke(this, new NextImageLoadedEventArgs
            {
                ImageIndex = currentImageIndex,
                ImageName = args.CoverImages[currentImageIndex]
            });
        }

        /// <summary>
        /// Write a binary string of all zeroes of the specified length to help skip the read/write
        /// position to a new desired position.
        /// </summary>
        /// <param name="pixelIndex">The number of pixels to skip through in the current image.</param>
        /// <exception cref="ImageProcessingException">Thrown if the value of position is greater than
        /// the number of available pixels the current image has.</exception>
        private void SeekToPixel(int bitsToSkip)
        {
            position.Reset();

            int pixelIndex = (int) Math.Ceiling((double) bitsToSkip / (double) Calculator.BitsPerPixel);
            for (int i = 0; i < pixelIndex; i++)
            {
                TryMoveToNextPixel();
            }
        }

        /// <summary>
        /// Tries to move the read/write x/y position to the next available pixel.
        /// Will return false if there are no more available pixels in the current image.
        /// </summary>
        /// <returns>False if there are no more pixels left to read/write to otherwise
        /// returns true.</returns>
        private bool TryMoveToNextPixel()
        {
            position.NextColumn();
            if (position.X == currentImage.Width)
            {
                position.NextRow();
                if (position.Y == currentImage.Height)
                {
                    position.Reset();
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Method to help close off any open images.
        /// </summary>
        /// <param name="saveImageChanges">If true this method will attempt to save any changes
        /// made to the current image.</param>
        private void CloseOpenImage(bool saveImageChanges = false)
        {
            if (currentImage != null)
            {
                if (saveImageChanges)
                {
                    currentImage.Save(args.CoverImages[currentImageIndex]);
                }
                currentImage.Dispose();
                currentImage = null;
            }
        }
    }

}