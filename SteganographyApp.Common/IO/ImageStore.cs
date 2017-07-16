using ImageSharp;
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

    /// <summary>
    /// Class that handles positioning a make shift write stream in the proper position so
    /// data can be reliably read and written to the storage images.
    /// </summary>
    public class ImageStore
    {

        /// <summary>
        /// Callback method for the Clean method to return the
        /// current progress back to the entry point so progress can be
        /// formatted and displayed to the user.
        /// </summary>
        public delegate void ProgressReport();

        /// <summary>
        /// The x position to start the next read/write operation from.
        /// </summary>
        private int x = 0;

        /// <summary>
        /// The y position to start the next read/write operation from.
        /// </summary>
        private int y = 0;

        /// <summary>
        /// The width of the image located at the current image index.
        /// This value is set whenever the Next method is called and the
        /// image index is incremented.
        /// </summary>
        private int currentImageWidth;

        /// <summary>
        /// The height of the image located at the current image index.
        /// This value is set whenever the Next method is called and the
        /// image index is incremented.
        /// </summary>
        private int currentImageHeight;

        /// <summary>
        /// The index used to determine which image will be read/written to in the
        /// next read/write operation.
        /// <para>The image name is retrieved by looking up the args.CoverImage value
        /// using this field as the index.</para>
        /// </summary>
        private int currentImageIndex = -1;

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
                int start = (int)(Math.Ceiling((double)(new FileInfo(args.FileToEncode).Length) / args.ChunkByteSize));
                RequiredContentChunkTableBitSize = start * ChunkDefinitionBitSize + ChunkDefinitionBitSize + start;
            }
        }

        /// <summary>
        /// Will look over all images specified in the InputArguments
        /// and set the LSB in all pixels in all images to 0.
        /// </summary>
        public void CleanAll(ProgressReport report)
        {
            foreach (string imagePath in args.CoverImages)
            {
                using (var image = Image.Load(imagePath))
                {
                    currentImageWidth = image.Width;
                    currentImageHeight = image.Height;

                    using (var pixels = image.Lock())
                    {
                        while (true)
                        {
                            byte newRed = (byte)(pixels[x, y].R & ~1);
                            byte newGreen = (byte)(pixels[x, y].G & ~1);
                            byte newBlue = (byte)(pixels[x, y].B & ~1);

                            pixels[x, y] = new Rgba32(newRed, newGreen, newBlue, pixels[x, y].A);

                            if(!TryMove())
                            {
                                break;
                            }
                        }
                    }
                    image.Save(imagePath);
                }
                report?.Invoke();
            }
        }

        /// <summary>
        /// Writes a binary string value to the current image, starting
        /// at the last write index, by replacing the LSB of each RGB value in each pixel.
        /// <para>If there is not enough write space on the image it will all as many bits
        /// as possible in the available space and return the total number of bits that this
        /// method successfully wrote to the image.</para>
        /// <para>This will increment the read/write position as each 3rd bit is written to the image.</para>
        /// </summary>
        /// <param name="binary">The encypted binary string to write to the image.</param>
        /// <returns>The number of bits written to the image.</returns>
        public int Write(string binary)
        {
            int written = 0;
            using (var image = Image.Load(args.CoverImages[currentImageIndex]))
            {
                using (var pixels = image.Lock())
                {
                    for (int i = 0; i < binary.Length; i += 3)
                    {
                        Rgba32 pixel = pixels[x, y];

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

                        pixels[x, y] = new Rgba32(
                                pixel.R,
                                pixel.G,
                                pixel.B,
                                pixel.A
                            );

                        if(!TryMove())
                        {
                            break;
                        }
                    }
                }
                image.Save(args.CoverImages[currentImageIndex]);
            }
            return written;
        }

        /// <summary>
        /// Attempts to read the specified number of bits from the current image starting
        /// at the last read/write position.
        /// <para>If there is not enough available space to read the specified number of bits
        /// from the image then this method will read all the remaining bits and retun the result.
        /// The length of the returned binary string can be used to determine how many bits were missed
        /// in the read operation.</para>
        /// <para>This will advance the read/write position as each 3rd bit is read from the
        /// image.</para>
        /// </summary>
        /// <param name="length">Specifies the number of bits to be read from the current image.</param>
        /// <returns>A binary string whose length is either the input length or a number based on the amount
        /// of readable bits remaining in the image.</returns>
        public string Read(int length)
        {
            var binary = new StringBuilder();
            using (var image = Image.Load(args.CoverImages[currentImageIndex]))
            {
                int read = 0;
                using (var pixels = image.Lock())
                {
                    while (read < length)
                    {
                        Rgba32 pixel = pixels[x, y];

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

                        if(!TryMove())
                        {
                            break;
                        }
                    }
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
            if(binary.Length < written)
            {
                throw new ImageProcessingException("There is not enough space in the current image to write the entire content chunk table.");
            }
        }

        /// <summary>
        /// Checks to see if the current image has enough available storage space to store the entirety
        /// of the content chunk table.
        /// </summary>
        /// <returns>Returns true if the number of bits the image can store is more than the estimated number of
        /// bits required for the content chunk table.</returns>
        public bool HasEnoughSpaceForContentChunkTable()
        {
            using (var image = Image.Load(args.CoverImages[currentImageIndex]))
            {
                using (var pixels = image.Lock())
                {
                    return (pixels.Width * pixels.Height * 3) > RequiredContentChunkTableBitSize;
                }
            }
        }

        /// <summary>
        /// Moves the current index back to the index before the specified image
        /// then calls the Next method to advance to the specified image.
        /// </summary>
        /// <param name="imageName">The image name to start reading and writing from.</param>
        public void ResetTo(string imageName)
        {
            currentImageIndex = Array.IndexOf(args.CoverImages, imageName) - 1;
            Next();
        }

        /// <summary>
        /// Resets the read/write position back to zero, generates a new
        /// read/write range, and moves the current read/write image to the next
        /// available image as determined by the args.CoverImages value.
        /// </summary>
        /// <exception cref="ImageProcessingException">If the increment of the current
        /// image index exeeds the available number of images an exception will be thrown.</exception>
        public void Next()
        {
            x = 0;
            y = 0;
            currentImageIndex++;
            if(currentImageIndex == args.CoverImages.Length)
            {
                throw new ImageProcessingException("There are not enough available store space in the provided images to process this request.");
            }

            using (var image = Image.Load(args.CoverImages[currentImageIndex]))
            {
                currentImageWidth = image.Width;
                currentImageHeight = image.Height;
            }
        }

        /// <summary>
        /// Write a binary string of all zeroes of the specified length to help skip the read/write
        /// position to a new desired position.
        /// </summary>
        /// <param name="length">The length of the all zero string to write to the current image.</param>
        public void Seek(int length)
        {
            x = 0;
            y = 0;
            int count = 0;
            while(count < length)
            {
                count += 3;
                x++;
                if(x == currentImageWidth)
                {
                    x = 0;
                    y++;
                    if(y == currentImageHeight)
                    {
                        throw new ImageProcessingException(String.Format("There are not enough available bits in this image to seek to the specified length of {0}", length));
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
            if(x == currentImageWidth)
            {
                x = 0;
                y++;
                if(y == currentImageHeight)
                {
                    x = 0;
                    y = 0;
                    return false;
                }
            }
            return true;
        }

    }

}