using SteganographyApp.Common;
using SteganographyApp.Common.Arguments;
using System;

namespace SteganographyAppCalculator
{

    /// <summary>
    /// Static reference class containing the number of available storage bits
    /// provided by images in common resolutions.
    /// </summary>
    public struct CommonResolutionStorageSpace
    {
        public static readonly int P360 = 518_400;
        public static readonly int P480 = 921_600;
        public static readonly int P720 = 2_764_800;
        public static readonly int P1080 = 6_220_800;
        public static readonly int P1440 = 11_059_200;
        public static readonly int P2160 = 25_012_800;
    }

    class Program
    {

        private static readonly ActionEnum[] CalculateEncryptedSizeActions = new ActionEnum[] { ActionEnum.CalculateEncryptedSize, ActionEnum.CES };
        private static readonly ActionEnum[] CalculateStorageSpaceActions = new ActionEnum[] { ActionEnum.CalculateStorageSpace, ActionEnum.CSS };

        static void Main(string[] args)
        {
            Console.WriteLine("\nSteganography Calculator\n");
            if (Checks.WasHelpRequested(args))
            {
                PrintHelp();
                return;
            }

            var parser = new ArgumentParser();
            if(!parser.TryParse(args, out IInputArguments arguments, PostValidate))
            {
                parser.PrintCommonErrorMessage();
                return;
            }

            if (IsCalculateStorageSpaceAction(arguments.EncodeOrDecode))
            {
                StorageSpaceCalculator.CalculateStorageSpace(arguments);
            }
            else if (IsCalculateEncryptedSpaceAction(arguments.EncodeOrDecode))
            {
                EncryptedSizeCalculator.CalculateEncryptedSize(arguments);
            }

            Console.WriteLine("");
        }

        private static bool IsCalculateStorageSpaceAction(ActionEnum action)
        {
            return Array.IndexOf(CalculateStorageSpaceActions, action) != -1;
        }

        private static bool IsCalculateEncryptedSpaceAction(ActionEnum action)
        {
            return Array.IndexOf(CalculateEncryptedSizeActions, action) != -1;
        }

        /// <summary>
        /// Performs some validation once all the user inputted values have been parsed and individually
        /// validated.
        /// </summary>
        private static string PostValidate(IInputArguments input)
        {
            if (!IsCalculateEncryptedSpaceAction(input.EncodeOrDecode) && !IsCalculateStorageSpaceAction(input.EncodeOrDecode))
            {
                return "The action must either be calculate-storage-space/css or calculate-encrypted-size/ces.";
            }
            else if (IsCalculateEncryptedSpaceAction(input.EncodeOrDecode))
            {
                if (Checks.IsNullOrEmpty(input.FileToEncode))
                {
                    return "A file must be specified in order to calculate the encrypted file size.";
                }
                else if (input.InsertDummies && Checks.IsNullOrEmpty(input.CoverImages))
                {
                    return "When insertDummies has been specified you must also provide at least one image "
                        + "to properly calculate the number of dummy entries to insert.";
                }
            }
            else if (IsCalculateStorageSpaceAction(input.EncodeOrDecode) && Checks.IsNullOrEmpty(input.CoverImages))
            {
                return "At least one image must be specified in order to calculate the available storage space of those images.";
            }

            return null;
        }

        /// <summary>
        /// Attempts to print the help information retrieved from the help.prop file.
        /// </summary>
        private static void PrintHelp()
        {
            var parser = new HelpParser();
            if (!parser.TryParseHelpFile(out HelpInfo info))
            {
                parser.PrintCommonErrorMessage();
                return;
            }

            Console.WriteLine("SteganographyApp Help\n");

            foreach (string message in info.GetHelpMessagesFor(HelpItemSet.Calculator))
            {
                Console.WriteLine("{0}\n", message);
            }
        }

    }
}