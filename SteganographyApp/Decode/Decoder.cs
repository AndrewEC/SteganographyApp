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

    /// <summary>
    /// Used to indicate if there is more data to be read from the storage images.
    /// Incomplete means there is more data to be read, complete means there is none.
    /// </summary>
    enum Status
    {
        Incomplete,
        Complete
    }

    public class Decoder
    {

        private readonly IInputArguments arguments;

        /// <summary>
        /// This queue allows for communication between the decoder and the write
        /// thread. Once each content chunk is read from the ImageStore it will be
        /// added to the this collection so the raw binary content from the image can
        /// be decoded and written to the decoded output file location.
        /// </summary>
        private readonly BlockingCollection<WriteArgs> writeQueue;

        /// <summary>
        /// This queue allows the file write thread to communicate errors back to the
        /// main thread so we can properly fail and throw an error whenever we
        /// fail to decode any of the binary content.
        /// </summary>
        private readonly BlockingCollection<Exception> errorQueue;

        private Decoder(IInputArguments arguments)
        {
            this.arguments = arguments;
            writeQueue = new BlockingCollection<WriteArgs>(2);
            errorQueue = new BlockingCollection<Exception>(1);
        }

        public static void CreateAndDecode(IInputArguments arguments)
        {
            new Decoder(arguments).DecodeFileFromImage();
        }

        /// <summary>
        /// Inititates the process of reading a file from an image, decoding it, and writing it
        /// to the target output file.
        /// The read/decode/write loop consists of the Decoder class reading the encoded binary
        /// content from the images and adding it to the writeQueue. The file write thread then
        /// picks up the raw content from the queue, decodes it, and writes it to the target output file.
        /// </summary>
        private void DecodeFileFromImage()
        {
            Console.WriteLine("Decoding to File: {0}", arguments.DecodedOutputFile);

            var store = new ImageStore(arguments);

            using (var wrapper = store.CreateIOWrapper())
            {
                var thread = FileWriteThread.CreateAndStartThread(writeQueue, errorQueue, arguments);

                var contentChunkTable = store.ReadContentChunkTable();
                var tracker = ProgressTracker.CreateAndDisplay(contentChunkTable.Length,
                    "Encoding file contents", "All input file contents have been decoded.");

                foreach (int chunkLength in contentChunkTable)
                {
                    string binary = wrapper.ReadContentChunkFromImage(chunkLength);
                    writeQueue.Add(new WriteArgs { Data = binary, Status = Status.Incomplete });
                    tracker.UpdateAndDisplayProgress();

                    if (errorQueue.TryTake(out Exception exception1))
                    {
                        thread.Join();
                        throw exception1;
                    }
                }

                writeQueue.Add(new WriteArgs { Status = Status.Complete });
                thread.Join();
            }

            Console.WriteLine("Decoding process complete.");
        }

    }

}