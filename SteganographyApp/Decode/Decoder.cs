using SteganographyApp.Common.Arguments;
using SteganographyApp.Common.IO;
using SteganographyApp.Common;

using System;
using System.Collections.Concurrent;

namespace SteganographyApp.Decode
{

    struct WriteArgs
    {
        public Status Status;
        public string Data;
    }

    enum Status
    {
        Incomplete,
        Complete
    }

    public class Decoder
    {

        private readonly IInputArguments arguments;
        private readonly BlockingCollection<WriteArgs> writeQueue;

        public Decoder(IInputArguments arguments)
        {
            this.arguments = arguments;
            writeQueue = new BlockingCollection<WriteArgs>(2);
        }

        public void DecodeFileFromImage()
        {
            Console.WriteLine("Decoding to File: {0}", arguments.DecodedOutputFile);

            var store = new ImageStore(arguments);
            var imageTracker = ImageTracker.CreateTrackerFrom(arguments, store);

            using (var wrapper = store.CreateIOWrapper())
            {
                new FileWriteThread(writeQueue, arguments).StartWriting();

                var contentChunkTable = store.ReadContentChunkTable();
                var tracker = ProgressTracker.CreateAndDisplay(contentChunkTable.Length,
                    "Encoding file contents", "All input file contents have been decoded.");

                foreach (int chunkLength in contentChunkTable)
                {
                    string binary = wrapper.ReadContentChunkFromImage(chunkLength);
                    writeQueue.Add(new WriteArgs { Data = binary, Status = Status.Incomplete });
                    tracker.UpdateAndDisplayProgress();
                }
            }
            writeQueue.Add(new WriteArgs { Status = Status.Complete });

            Console.WriteLine("Decoding process complete.");
        }

    }

}