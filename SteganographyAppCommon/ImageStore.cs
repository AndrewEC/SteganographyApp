using ImageSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SteganographyAppCommon
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

        private int[] rowPositions;
        private int[] colPositions;

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
        /// The values parsed from the command line arguments.
        /// </summary>
        private readonly InputArguments args;

        /// <summary>
        /// The index used to determine which image will be read/written to in the
        /// next read/write operation.
        /// <para>The image name is retrieved by looking up the args.CoverImage value
        /// using this field as the index.</para>
        /// </summary>
        private int currentImageIndex = -1;

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
                int start = (int)(Math.Ceiling((double)(new FileInfo(args.FileToEncode).Length) / ContentReader.ChunkByteSize));
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
                    using (var pixels = image.Lock())
                    {
                        int x = 0, y = 0;

                        while (true)
                        {
                            byte newRed = (byte)(pixels[x, y].R & ~1);
                            byte newGreen = (byte)(pixels[x, y].G & ~1);
                            byte newBlue = (byte)(pixels[x, y].B & ~1);

                            pixels[x, y] = new Rgba32(newRed, newGreen, newBlue, pixels[x, y].A);

                            x++;
                            if (x == image.Width)
                            {
                                x = 0;
                                y++;
                                if (y == image.Height)
                                {
                                    break;
                                }
                            }
                        }
                    }
                    image.Save(imagePath);
                }
                if(report != null)
                {
                    report();
                }
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
                        Rgba32 pixel = pixels[colPositions[x], rowPositions[y]];

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

                        pixels[colPositions[x], rowPositions[y]] = new Rgba32(
                                pixel.R,
                                pixel.G,
                                pixel.B,
                                pixel.A
                            );

                        x++;
                        if (x == image.Width)
                        {
                            x = 0;
                            y++;
                            if (y == image.Height)
                            {
                                break;
                            }
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
                        Rgba32 pixel = pixels[colPositions[x], rowPositions[y]];

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

                        x++;
                        if (x == image.Width)
                        {
                            x = 0;
                            y++;
                            if (y == image.Height)
                            {
                                break;
                            }
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
            int tableLength = Convert.ToInt32(Read(ChunkDefinitionBitSize), 2);
            var table = new List<int>(tableLength);
            for (int i = 0; i < tableLength; i++)
            {
                table.Add(Convert.ToInt32(Read(ChunkDefinitionBitSize), 2));
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
            string chunkCountBits = Convert.ToString(table.Count, 2).PadLeft(ChunkDefinitionBitSize, '0');
            Write(chunkCountBits);
            int count = 0;
            foreach (int chunkLength in table)
            {
                count++;
                string chunkLengthBits = Convert.ToString(chunkLength, 2).PadLeft(ChunkDefinitionBitSize, '0');
                Write(chunkLengthBits);
            }
        }

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
                colPositions = Enumerable.Range(0, image.Width).ToArray();
                rowPositions = Enumerable.Range(0, image.Height).ToArray();
            }
        }

        /// <summary>
        /// Write a binary string of all zeroes of the specified length to help skip the read/write
        /// position to a new desired position.
        /// </summary>
        /// <param name="length">The length of the all zero string to write to the current image.</param>
        public void Seek(int length)
        {
            Write(Convert.ToString('0').PadLeft(length, '0'));
        }

    }

}