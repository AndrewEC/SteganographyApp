using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading;

using SteganographyApp.Common;
using SteganographyApp.Common.Data;
using SteganographyApp.Common.Providers;
using SteganographyApp.Common.Arguments;

namespace SteganographyAppCalculator
{

    public static class EncryptedSizeCalculator
    {

        /// <summary>
        /// Calculate the total size of the input file after base64 conversion, binary conversion,
        /// and optionally encryption if a password argument was provided.
        /// </summary>
        /// <param name="args">The InputArguments instance parsed from the user provided command
        /// line arguments.</param>
        public static void CalculateEncryptedSize(IInputArguments args)
        {
            Console.WriteLine("Calculating encypted size of file {0}.", args.FileToEncode);
            try
            {
                double size = new Orchestrator(args).CalculateFileSize();

                Console.WriteLine("\nEncrypted file size is:");
                PrintSize(size);

                Console.WriteLine("\n# of images required to store this file at common resolutions:");
                PrintComparison(size);
            }
            catch (TransformationException e)
            {
                Console.WriteLine("An error occured while encoding file: {0}", e.Message);
                if (args.PrintStack)
                {
                    Console.WriteLine(e.StackTrace);
                }
            }
        }

        /// <summary>
        /// Prints out the binary size of an encypted file or storage space in bits, bytes,
        /// megabytes and gigabytes.
        /// </summary>
        /// <param name="size">The size in bits to print out.</param>
        private static void PrintSize(double size)
        {
            Console.WriteLine("\t{0} bits", size);
            Console.WriteLine("\t{0} bytes", size / 8);
            Console.WriteLine("\t{0} KB", size / 8 / 1024);
            Console.WriteLine("\t{0} MB", size / 8 / 1024 / 1024);
        }

        /// <summary>
        /// Prints how many images at common resolutions it would take to store this content.
        /// </summary>
        /// <param name="size">The size of the encoded file in bits.</param>
        private static void PrintComparison(double size)
        {
            Console.WriteLine("\tAt 360p: \t{0}", size / CommonResolutionStorageSpace.P360);
            Console.WriteLine("\tAt 480p: \t{0}", size / CommonResolutionStorageSpace.P480);
            Console.WriteLine("\tAt 720p: \t{0}", size / CommonResolutionStorageSpace.P720);
            Console.WriteLine("\tAt 1080p: \t{0}", size / CommonResolutionStorageSpace.P1080);
            Console.WriteLine("\tAt 1440p: \t{0}", size / CommonResolutionStorageSpace.P1440);
            Console.WriteLine("\tAt 4K (2160p): \t{0}", size / CommonResolutionStorageSpace.P2160);
        }

    }

    /// <summary>
    /// Orchestrates the running of multiple threads for the purpose of calculating the size of
    /// the given file to encode.
    /// </summary>
    class Orchestrator
    {

        private static readonly object _lock = new object();
        private static readonly int MaxThreadCount = 6;

        private readonly ProgressTracker progressTracker;
        private readonly int requiredNumberOfWrites;
        private readonly IInputArguments arguments;

        private readonly Dictionary<int, double> chunkLengths;
        private readonly Queue<int> idsToCheckout;

        private readonly CountdownEvent countdownEvent;

        public Orchestrator(IInputArguments arguments)
        {
            this.arguments = arguments;

            requiredNumberOfWrites = Calculator.CalculateRequiredNumberOfWrites(arguments.FileToEncode, arguments.ChunkByteSize);
            progressTracker = ProgressTracker.CreateAndDisplay(requiredNumberOfWrites,
                    "Calculating file size", "Completed calculating file size");

            countdownEvent = new CountdownEvent(requiredNumberOfWrites);
            chunkLengths = new Dictionary<int, double>(requiredNumberOfWrites);
            idsToCheckout = new Queue<int>(requiredNumberOfWrites);
            for (int i = 0; i < requiredNumberOfWrites; i++)
            {
                idsToCheckout.Enqueue(i);
                chunkLengths[i] = -1;
            }
        }

        /// <summary>
        /// Start the calculation threads and return a summation of all the chunk sizes calculated by
        /// the threads.
        /// </summary>
        public double CalculateFileSize()
        {
            var calculationThreads = CreateAndStartThreads();
            WaitForThreadsToFinish(calculationThreads);
            return chunkLengths.Values.Sum();
        }

        private ChunkSizeCalculatorThread[] CreateAndStartThreads()
        {
            var calculationThreads = Enumerable.Range(0, GetThreadCount())
                .Select(i => new ChunkSizeCalculatorThread(arguments, this))
                .ToArray();

            foreach (var thread in calculationThreads)
            {
                thread.Start();
            }

            return calculationThreads;
        }

        private void WaitForThreadsToFinish(ChunkSizeCalculatorThread[] calculationThreads)
        {
            countdownEvent.Wait();

            foreach (var thread in calculationThreads)
            {
                thread.Join();
            }
        }

        /// <summary>
        /// Retrieves the number of thread appropriate to handle the calculation of the
        /// encoded file size. The number of threads will be between 1 and MaxThreadCount inclusive.
        /// </summary>
        private int GetThreadCount()
        {
            if (requiredNumberOfWrites >= MaxThreadCount)
            {
                return MaxThreadCount;
            }
            return requiredNumberOfWrites;
        }

        /// <summary>
        /// Attempts to pull an ID of the next chunk whose encoded size needs
        /// to be calculated. If there are no more chunks left needing calculation
        /// then this will return -1 signaling the calculator thread to stop execution.
        /// </summary>
        public int CheckoutChunkId()
        {
            lock (_lock)
            {
                if (idsToCheckout.Count == 0)
                {
                    return -1;
                }
                return idsToCheckout.Dequeue();
            }
        }

        /// <summary>
        /// </summary>
        public void CheckinChunk(int chunkId, double chunkLength)
        {
            lock (_lock)
            {
                chunkLengths[chunkId] = chunkLength;
                progressTracker.UpdateAndDisplayProgress();
                countdownEvent.Signal();
            }
        }

    }

    /// <summary>
    /// Threadholder that will handle reading in and encoding a portion, chunk, of the
    /// file to encode.
    /// </summary>
    class ChunkSizeCalculatorThread
    {

        private readonly IInputArguments arguments;
        private readonly Orchestrator orchestrator;
        private readonly IDataEncoderUtil encoder;
        private readonly Thread thread;

        public ChunkSizeCalculatorThread(IInputArguments arguments, Orchestrator orchestrator)
        {
            this.arguments = arguments;
            this.orchestrator = orchestrator;
            encoder = Injector.Provide<IDataEncoderUtil>();
            thread = new Thread(new ThreadStart(StartCalculating));
        }

        public void Start()
        {
            thread.Start();
        }

        private void StartCalculating()
        {
            while (true)
            {
                int chunkId = orchestrator.CheckoutChunkId();
                if (chunkId == -1)
                {
                    break;
                }

                int chunkLength = CalculateChunkLength(chunkId);
                orchestrator.CheckinChunk(chunkId, chunkLength);
            }
        }

        /// <summary>
        /// Read in a chunk of the file to encode and pass the read in bytes to the
        /// encoder util.
        /// </summary>
        /// <param name="chunkId">Used to specify where the thread will start and stop reading.
        /// The thread will use the equation chunkId * arguments.ChunkByteSize
        /// to determine where to start reading the file to encode from.</param>
        private int CalculateChunkLength(int chunkId)
        {
            byte[] buffer = new byte[arguments.ChunkByteSize];
            using (var stream = File.OpenRead(arguments.FileToEncode))
            {
                stream.Seek(chunkId * arguments.ChunkByteSize, SeekOrigin.Begin);
                int read = stream.Read(buffer, 0, buffer.Length);
                if (read < arguments.ChunkByteSize)
                {
                    byte[] actual = new byte[read];
                    Array.Copy(buffer, actual, read);
                    buffer = actual;
                }
            }

            return encoder.Encode(buffer, arguments.Password, arguments.UseCompression, arguments.DummyCount, "").Length;
        }

        public void Join()
        {
            try
            {
                thread.Join();
            }
            catch (Exception)
            {
            }
        }

    }

}