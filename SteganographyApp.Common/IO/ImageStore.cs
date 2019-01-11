using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SteganographyApp.Common.IO.Content;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SteganographyApp.Common.IO
{

    /// <summary>
    /// A general exception to represent a specific error occured
    /// while reading or writing data to the images.
    /// </summary>
    public class ImageProcessingException : Exception
    {
        public ImageProcessingException(string message) : base(message) { }
        public ImageProcessingException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary>
    /// Static reference class containing the number of available storage bits
    /// provided by images in common resolutions.
    /// </summary>
    public static class CommonResolutionStorageSpace
    {
        public static readonly int P360 = 518_400;
        public static readonly int P480 = 921_600;
        public static readonly int P720 = 2_764_800;
        public static readonly int P1080 = 6_220_800;
        public static readonly int P1440 = 11_059_200;
        public static readonly int P2160 = 25_012_800;
    }

    public class NextImageLoadedEventArgs
    {
        public string ImageName { get; set; }
        public int ImageIndex { get; set; }
    }

    /// <summary>
    /// Class that handles positioning a make shift write stream in the proper position so
    /// data can be reliably read and written to the storage images.
    /// </summary>
    public class ImageStore
    {

        /// <summary>
        /// Event handle the will be invoked whenever the Next method is internally
        /// invoked so that the calling entry point can record which images
        /// have been used in the encoding or decoding process.
        /// </summary>
        public event EventHandler<NextImageLoadedEventArgs> OnNextImageLoaded;

        /// <summary>
        /// The x position to start the next read/write operation from.
        /// </summary>
        private int x = 0;

        /// <summary>
        /// The y position to start the next read/write operation from.
        /// </summary>
        private int y = 0;

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
        private Image<Rgba32> currentImage;

        /// <summary>
        /// The values parsed from the command line arguments.
        /// </summary>
        private readonly InputArguments args;

        /// <summary>
        /// Specifies the number of bits that will be reserved for each entry in the content
        /// chunk table.
        /// </summary>
        private readonly int ChunkDefinitionBitSize = 32;

        /// <summary>
        /// Specifies the number of bits that will be required to write the content chunk table to
        /// the leading image.
        /// <para>If no FileToEncode has been specified in the args field then this value will
        /// always be 0.</para>
        /// </summary>
        public int RequiredContentChunkTableBitSize { get; private set; }

        /// <summary>
        /// Readonly property that returns the name of the current image being used in the write
        /// process.
        /// </summary>
        public string CurrentImage
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
        public ImageStore(InputArguments args)
        {
            this.args = args;

            if (args.FileToEncode.Length > 0)
            {
                // Specifies the number of times the file has to be read from, encoded, and written to the storage
                // image. The number of writes is essentially based on the total size of the image divided by the
                // number of bytes to read from each iteration from the input file.
                int requiredWrites = (int)(Math.Ceiling((double)(new FileInfo(args.FileToEncode).Length) / args.ChunkByteSize));
                // The table size is essentially the number of read/encode/write iterations times 32.
                // Each time a chunk is read and encoded, the size of the encoded/compressed chunk is written
                // to the table so it can be read and decoded later.
                // We add an additional 32 bits onto the end so that a table header can be written that
                // specifies the number of read write iterations that occurred when encoding the file
                // so that it can be properly decoded.
                RequiredContentChunkTableBitSize = requiredWrites * ChunkDefinitionBitSize + ChunkDefinitionBitSize + requiredWrites;
            }
        }

        /// <summary>
        /// Will look over all images specified in the InputArguments
        /// and set the LSB in all pixels in all images to 0.
        /// </summary>
        public void CleanAll()
        {
            currentImageIndex = -1;
            Next();
            for(int i = 0; i < args.CoverImages.Length; i++)
            {
                while (true)
                {
                    byte newRed = (byte)(currentImage[x, y].R & ~1);
                    byte newGreen = (byte)(currentImage[x, y].G & ~1);
                    byte newBlue = (byte)(currentImage[x, y].B & ~1);

                    currentImage[x, y] = new Rgba32(newRed, newGreen, newBlue, currentImage[x, y].A);

                    if (!TryMove())
                    {
                        break;
                    }
                }
                if (i == args.CoverImages.Length - 1)
                {
                    Finish(true);
                }
                else
                {
                    Next(true);
                }
            }
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
        public int Write(string binary)
        {
            int written = 0;
            for (int i = 0; i < binary.Length; i += 3)
            {
                Rgba32 pixel = currentImage[x, y];

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

                currentImage[x, y] = new Rgba32(
                        pixel.R,
                        pixel.G,
                        pixel.B,
                        pixel.A
                    );

                if (!TryMove())
                {
                    Next(true);
                }
            }
            return written;
        }

        /// <summary>
        /// Attempts to read the specified number of bits from the current image starting
        /// at the last read/write position.
        /// <para>If there is not enough available space in the current image then it will
        /// invoke the Next method and continue to read from the next available image.</para>
        /// </summary>
        /// <param name="length">Specifies the number of bits to be read from the current image.</param>
        /// <returns>A binary string whose length is equal to the length specified in the length
        /// parameter.</returns>
        /// <exception cref="ImageProcessingException">Rethrown from the Next method call.</exception>
        public string Read(int length)
        {
            var binary = new StringBuilder();
            int read = 0;
            while (read < length)
            {
                Rgba32 pixel = currentImage[x, y];

                string redBit = Convert.ToString(pixel.R, 2);
                binary.Append(redBit.Substring(redBit.Length - 1));
                read++;

                if (read < length)
                {
                    string greenBit = Convert.ToString(pixel.G, 2);
                    binary.Append(greenBit.Substring(greenBit.Length - 1));
                    read++;

                    if (read < length)
                    {
                        string blueBit = Convert.ToString(pixel.B, 2);
                        binary.Append(blueBit.Substring(blueBit.Length - 1));
                        read++;
                    }
                }

                if (!TryMove())
                {
                    Next();
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
        /// <exception cref="ImageProcessingException">Thrown if the leading image does not have enough
        /// storage space to read the entire content chunk table.</exception>
        public List<int> ReadContentChunkTable()
        {
            //The size of each table entry plus the one padding bit to fill out a full pixel
            int chunkSizeAndPadding = ChunkDefinitionBitSize + 1;

            //Read the number of entries in the table
            int tableLength = Convert.ToInt32(Read(ChunkDefinitionBitSize), 2);

            int tableBitCount = tableLength * chunkSizeAndPadding;

            //Read all the available entries in the table
            string rawTable = Read(tableBitCount);

            if (rawTable.Length < tableBitCount)
            {
                throw new ImageProcessingException("There are not enough available bits in the image to read the entire content chunk table.");
            }

            var table = new List<int>();
            for (int i = 0; i < rawTable.Length; i += chunkSizeAndPadding)
            {
                // Although we have 33 bits read for the table entry the last bit of the table entry is purely
                // padding so we need to substring 33 bits minus the one padding bit.
                int entry = Convert.ToInt32(rawTable.Substring(i, ChunkDefinitionBitSize), 2);
                table.Add(entry);
            }
            return table;
        }

        /// <summary>
        /// Writes the content chunk table so that the proper number of bits can be read
        /// from the target images and properly decrypted.
        /// </summary>
        /// <param name="table">A list containing the bit length of each chunk of data that was
        /// originally written to the target images during the encoding process.</param>
        /// <exception cref="ImageProcessingException">Thrown if the leading image does not have enough
        /// storage space to store the entire content chunk table.</exception>
        public void WriteContentChunkTable(List<int> table)
        {
            var binary = new StringBuilder("");

            // Each table entry is 32 bits in size meaning that since each pixel can store 3 bits it will take
            // 11 pixels. Since 11 pixels can actually store 33 bits we pad the 32 bit table entry with an additional
            // zero at the end which will be ignored when reading the table.
            binary.Append(Convert.ToString(table.Count, 2).PadLeft(ChunkDefinitionBitSize, '0')).Append('0');

            foreach (int chunkLength in table)
            {
                binary.Append(Convert.ToString(chunkLength, 2).PadLeft(ChunkDefinitionBitSize, '0')).Append('0');
            }

            int written = Write(binary.ToString());

            if (binary.Length < written)
            {
                throw new ImageProcessingException("There is not enough space in the current image to write the entire content chunk table.");
            }

            Finish(true);
        }

        /// <summary>
        /// Checks to see if the current image has enough available storage space to store the entirety
        /// of the content chunk table.
        /// </summary>
        /// <returns>Returns true if the number of bits the image can store is more than the estimated number of
        /// bits required for the content chunk table.</returns>
        public bool HasEnoughSpaceForContentChunkTable()
        {
            return (currentImage.Width * currentImage.Height * 3) > RequiredContentChunkTableBitSize;
        }

        /// <summary>
        /// Moves the current index back to the index before the specified image
        /// then calls the Next method to advance to the specified image.
        /// </summary>
        /// <param name="imageName">The image name to start reading and writing from.</param>
        /// <exception cref="ImageProcessingException">Rethrown from the Next method call.</exception>
        public void ResetTo(int index)
        {
            if(index < 0 || index >= args.CoverImages.Length)
            {
                throw new ImageProcessingException(string.Format("An invalid image index was provided in ResetTo. Expected value between {0} and {1}, instead got {2}", 0, args.CoverImages.Length - 1, index));
            }
            currentImageIndex = index - 1;
            Next();
        }

        /// <summary>
        /// Attempts to save then dispose of the currently opened image, if the currentImage property
        /// is not null, resets the read/write x and y positions to 0, increments the current image index,
        /// loads the new image, and invokes the OnNextImageLoaded event.
        /// </summary>
        /// <param name="save">If true it will attempt to save any changes made to the currently open
        /// image before attempting to increment to the next available image.</param>
        /// <exception cref="ImageProcessingException">If the increment of the current
        /// image index exeeds the available number of images an exception will be thrown.</exception>
        public void Next(bool save = false)
        {
            if(currentImage != null)
            {
                if (save)
                {
                    currentImage.Save(args.CoverImages[currentImageIndex]);
                }
                currentImage.Dispose();
                currentImage = null;
            }

            x = 0;
            y = 0;
            currentImageIndex++;
            if (currentImageIndex == args.CoverImages.Length)
            {
                throw new ImageProcessingException("There are not enough available store space in the provided images to process this request.");
            }

            currentImage = Image.Load(args.CoverImages[currentImageIndex]);

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
        /// <param name="position">The number of pixels to skip through in the current image.</param>
        /// <exception cref="ImageProcessingException">Thrown if the value of position is greater than
        /// the number of available pixels the current image has.</exception>
        public void Seek(int position)
        {
            x = 0;
            y = 0;
            int count = 0;
            while (count < position)
            {
                count += 3;
                x++;
                if (x == currentImage.Width)
                {
                    x = 0;
                    y++;
                    if (y == currentImage.Height)
                    {
                        throw new ImageProcessingException(String.Format("There are not enough available bits in this image to seek to the specified length of {0}", position));
                    }
                }
            }
        }

        /// <summary>
        /// Tries to move the read/write x/y position to the next available pixel.
        /// Will return false if there are no more available pixels in the current image.
        /// </summary>
        /// <returns>False if there are no more pixels left to read/write to otherwise
        /// returns true.</returns>
        public bool TryMove()
        {
            x++;
            if (x == currentImage.Width)
            {
                x = 0;
                y++;
                if (y == currentImage.Height)
                {
                    x = 0;
                    y = 0;
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Method to help close off any open images.
        /// </summary>
        /// <param name="save">If true this method will attempt to save any changes
        /// made to the current image.</param>
        public void Finish(bool save = false)
        {
            if(currentImage != null)
            {
                if (save)
                {
                    currentImage.Save(args.CoverImages[currentImageIndex]);
                }
                currentImage.Dispose();
                currentImage = null;
            }
        }
    }

}