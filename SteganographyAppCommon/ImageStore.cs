using ImageSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SteganographyAppCommon
{

    public class ImageStore
    {

        private int[] rowPositions;
        private int[] colPositions;
        private int x = 0;
        private int y = 0;
        private readonly InputArguments args;
        private int currentImageIndex = -1;
        private readonly int ChunkDefinitionBitSize = 32;
        public int RequiredContentChunkTableBitSize { get; private set; }
        public bool IsLeadingImage { get; private set; }

        public ImageStore(InputArguments args)
        {
            this.args = args;

            if (args.FileToEncode.Length > 0)
            {
                int start = (int)(Math.Ceiling((double)(new FileInfo(args.FileToEncode).Length) / ContentReader.ChunkByteSize));
                RequiredContentChunkTableBitSize = start * ChunkDefinitionBitSize + ChunkDefinitionBitSize + start;
            }
        }

        public void CleanAll()
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
            }
        }

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

        public void ResetTo(string imageName, bool randomAccess)
        {
            currentImageIndex = Array.IndexOf(args.CoverImages, imageName) - 1;
            Next(randomAccess);
        }

        public void Next(bool randomAccess)
        {
            x = 0;
            y = 0;
            currentImageIndex++;
            IsLeadingImage = (currentImageIndex == 0) ? true : false;

            using (var image = Image.Load(args.CoverImages[currentImageIndex]))
            {
                colPositions = Enumerable.Range(0, image.Width).ToArray();
                rowPositions = Enumerable.Range(0, image.Height).ToArray();
            }
        }

        public void WriteAll0(int length)
        {
            Write(Convert.ToString('0').PadLeft(length, '0'));
        }

    }

}