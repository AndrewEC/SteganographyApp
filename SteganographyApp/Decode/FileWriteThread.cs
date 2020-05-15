using SteganographyApp.Common.Arguments;
using SteganographyApp.Common.IO.Content;

using System;
using System.Collections.Concurrent;
using System.Threading;

namespace SteganographyApp.Decode
{

    /// <summary>
    /// Handles taking in the raw binary data read from an image, decoding it,
    /// and writing it to the target location.
    /// </summary>
    class FileWriteThread
    {

        private readonly BlockingCollection<WriteArgs> queue;
        private readonly DecodeError decodeError;
        private readonly IInputArguments arguments;
        private Thread readThread;

        private FileWriteThread(BlockingCollection<WriteArgs> queue, DecodeError decodeError, IInputArguments arguments)
        {
            this.arguments = arguments;
            this.queue = queue;
            this.decodeError = decodeError;
        }

        public static FileWriteThread CreateAndStartThread(BlockingCollection<WriteArgs> queue, DecodeError decodeError,
            IInputArguments arguments)
        {
            var thread = new FileWriteThread(queue, decodeError, arguments);
            thread.StartWriting();
            return thread;
        }

        public void StartWriting()
        {
            readThread = new Thread(new ThreadStart(Write));
            readThread.Start();
        }

        /// <summary>
        /// Here the thread will wait to be able to take an item from the
        /// queue. The queue item will be the raw binary string read the from
        /// images storing the content. This thread will then take that content,
        /// decode it, and write it to the destination file.
        /// If an exception ocurrs while reading from the source file the exception will be added
        /// to the erro queue and the loop will exit.
        /// </summary>
        private void Write()
        {
            using (var writer = new ContentWriter(arguments))
            {
                while (true)
                {
                    var writeArgs = queue.Take();
                    if (writeArgs.Status == Status.Complete)
                    {
                        break;
                    }
                    else
                    {
                        try
                        {
                            writer.WriteContentChunkToFile(writeArgs.Data);
                        }
                        catch (Exception e)
                        {
                            decodeError.PutException(e);
                            break;
                        }
                    }
                }
            }
        }

        public void Join()
        {
            try
            {
                readThread.Join();
            }
            catch {}
        }

    }

}