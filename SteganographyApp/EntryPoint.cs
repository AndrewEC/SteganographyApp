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
            var store = new ImageStore(args);
            int start = store.RequiredContentChunkTableBitSize;
            store.Next();
            store.WriteAll0(start);
            var table = new List<int>();
            using(var reader = new ContentReader(args))
            {
                string content = "";
                while((content = reader.ReadNextChunk()) != null)
                {
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
            }
            store.ResetTo(args.CoverImages[0]);
            store.WriteContentChunkTable(table);
        }

        private void StartDecode()
        {
            var store = new ImageStore(args);
            store.Next();
            var chunkTable = store.ReadContentChunkTable();
            var less = 0;
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
                            binary = "";
                            stillReading = false;
                        }
                    }
                }
            }
        }

    }
}
