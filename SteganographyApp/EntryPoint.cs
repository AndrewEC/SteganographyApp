using SteganographyAppCommon;
using System;
using System.Collections.Generic;
using System.Text;

namespace SteganographyApp
{
    public class EntryPoint
    {

        private readonly InputArguments args;

        public EntryPoint(InputArguments args)
        {
            this.args = args;
        }

        public void Start()
        {
            switch (args.EncodeOrDecode)
            {
                case EncodeDecodeAction.Clean:
                    new ImageStore(args).CleanAll();
                    break;
                case EncodeDecodeAction.Encode:
                    StartEncode();
                    break;
                case EncodeDecodeAction.Decode:
                    StartDecode();
                    break;
            }
        }

        private void StartEncode()
        {
            Console.WriteLine("Started encoding file {0}", args.FileToEncode);

            var store = new ImageStore(args);
            int start = store.RequiredContentChunkTableBitSize;
            store.Next();
            store.WriteAll0(start);
            var table = new List<int>();
            int chunksRead = 0; //used to display how much data has been read as a percent
            using(var reader = new ContentReader(args))
            {
                string content = "";
                while((content = reader.ReadNextChunk()) != null)
                {
                    chunksRead++; //used to display how much data has been read as a percent
                    DisplayPercent(chunksRead, reader.RequiredNumberOfReads, "Encoding file contents"); //used to display how much data has been read as a percent
                    table.Add(content.Length);
                    bool stillWriting = true;
                    while (stillWriting)
                    {
                        int wrote = store.Write(content);
                        if (wrote < content.Length)
                        {
                            content = content.Substring(wrote);
                            store.Next();
                        }
                        else
                        {
                            stillWriting = false;
                        }
                    }
                }

                Console.WriteLine("All input file contents have been encoded.");
            }
            store.ResetTo(args.CoverImages[0]);
            Console.WriteLine("Writing content chunk table.");
            store.WriteContentChunkTable(table);
            Console.WriteLine("Encoding process complete.");
        }

        private void StartDecode()
        {
            Console.WriteLine("Decoding data to file {0}", args.DecodedOutputFile);

            var store = new ImageStore(args);
            store.Next();
            Console.WriteLine("Reading content chunk table.");
            var chunkTable = store.ReadContentChunkTable();
            var less = 0;
            int chunksWritten = 0; //used to display how much data has been read as a percent
            using (var writer = new ContentWriter(args))
            {
                foreach (int length in chunkTable)
                {
                    bool stillReading = true;
                    string binary = "";
                    while (stillReading)
                    {
                        binary += store.Read(length - less);
                        if (binary.Length < length)
                        {
                            less += binary.Length;
                            store.Next();
                        }
                        else
                        {
                            less = 0;
                            writer.WriteChunk(binary);
                            chunksWritten++; //used to display how much data has been read as a percent
                            DisplayPercent(chunksWritten, chunkTable.Count, "Decoding file contents"); //used to display how much data has been read as a percent
                            binary = "";
                            stillReading = false;
                        }
                    }
                }
                Console.WriteLine("All encoded file contents has been decoded.");
            }
            Console.WriteLine("Decoding process complete.");
        }

        private void DisplayPercent(double cur, double max, string prefix)
        {
            double percent = cur / max * 100.0;
            if(percent > 100.0)
            {
                percent = 100.0;
            }
            Console.Write("{0} :: {1}%\r", prefix, (int)percent);
        }

    }
}
