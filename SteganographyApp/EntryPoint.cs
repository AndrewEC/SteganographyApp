namespace SteganographyApp
{
    using SteganographyApp.Common;
    using SteganographyApp.Common.Arguments;
    using SteganographyApp.Common.Injection;
    using SteganographyApp.Common.IO;
    using SteganographyApp.Decode;
    using SteganographyApp.Encode;

    public class EntryPoint
    {
        /// <summary>
        /// The model with values parsed from the user's input.
        /// </summary>
        private readonly IInputArguments args;

        /// <summary>
        /// Instantiates a new InputArguments instance with user provided
        /// command line values.
        /// </summary>
        /// <param name="args">The model with values parsed from the user's input.</param>
        public EntryPoint(IInputArguments args)
        {
            this.args = args;
        }

        /// <summary>
        /// Delegate method to determine which action to take based on the EncodeOrDecode action.
        /// </summary>
        public void Start()
        {
            switch (args.EncodeOrDecode)
            {
                case ActionEnum.Clean:
                    CleanImages();
                    break;
                case ActionEnum.Encode:
                    EncodeFileToImages();
                    break;
                case ActionEnum.Decode:
                    DecodeImagesToFile();
                    break;
            }
        }

        /// <summary>
        /// Starts the cleaning process.
        /// Uses the image store to reset the LSB values in all the user provided
        /// images to a value of 0.
        /// </summary>
        private void CleanImages()
        {
            Injector.LoggerFor<EntryPoint>().Trace("Cleaning image LSBs");
            var tracker = ProgressTracker.CreateAndDisplay(args.CoverImages.Length, "Cleaning image LSB data", "Finished cleaning all images.");
            var store = new ImageStore(args);
            store.OnNextImageLoaded += (object? sender, NextImageLoadedEventArgs eventArg) =>
            {
                tracker.UpdateAndDisplayProgress();
            };
            store.CleanImageLSBs();
        }

        /// <summary>
        /// Starts the encoding process.
        /// Uses the ContentReader and ImageStore to read in chunks of the input file,
        /// encode the read content, write the content to the images, and then write the
        /// content chunk table to the leading image.
        /// </summary>
        private void EncodeFileToImages()
        {
            Encoder.CreateAndEncode(args);
        }

        /// <summary>
        /// Starts the decoding process.
        /// Reads the content chunk table, reads each chunk, decodes it, writes it to
        /// the output file.
        /// </summary>
        private void DecodeImagesToFile()
        {
            Decoder.CreateAndDecode(args);
        }
    }
}
