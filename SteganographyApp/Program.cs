using SteganographyApp.Common;
using SteganographyApp.Common.Data;
using SteganographyApp.Common.Arguments;
using System;

namespace SteganographyApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("\nSteganography App\n");
            if (Array.IndexOf(args, "--help") != -1 || Array.IndexOf(args, "-h") != -1)
            {
                PrintHelp();
                return;
            }

            var parser = new ArgumentParser();
            if(!parser.TryParse(args, out InputArguments arguments, PostValidate))
            {
                parser.PrintCommonErrorMessage();
                return;
            }

            try
            {
                new EntryPoint(arguments).Start();
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occured during execution: ");
                Console.WriteLine("\tException Message: {0}", e.Message);
                if (arguments.PrintStack)
                {
                    Console.WriteLine(e.StackTrace);
                }

                switch (e)
                {
                    case TransformationException t:
                        Console.WriteLine("This error often occurs as a result of an incorrect password or incorrect dummy count when decrypting a file.");
                        break;
                }
            }

            Console.WriteLine("");
        }

        /// <summary>
        /// Performs some validation once all the user inputted values have been parsed and individually
        /// validated.
        /// </summary>
        private static string PostValidate(InputArguments input)
        {
            if (!Checks.IsOneOf(input.EncodeOrDecode, EncodeDecodeAction.Clean, EncodeDecodeAction.Encode,
                EncodeDecodeAction.Decode))
            {
                return "The action specified must be one of: 'clean', 'encode', or 'decode'.";
            }
            if (input.EncodeOrDecode == EncodeDecodeAction.Encode && Checks.IsNullOrEmpty(input.FileToEncode))
            {
                return "Specified encode action but no file to encode was provided in arguments.";
            }
            else if (input.EncodeOrDecode == EncodeDecodeAction.Decode && Checks.IsNullOrEmpty(input.DecodedOutputFile))
            {
                return "Specified decode action but no file to decode was provided in arguments.";
            }
            else if ((input.EncodeOrDecode == EncodeDecodeAction.Encode || input.EncodeOrDecode == EncodeDecodeAction.Decode) && Checks.IsNullOrEmpty(input.CoverImages))
            {
                return "In order to encode or decode at least one image must be provided in the list of arguments.";
            }
            return null;
        }

        /// <summary>
        /// Attempts to print out the help information retrieved from the help.prop file.
        /// </summary>
        static void PrintHelp()
        {
            var parser = new HelpParser();
            if (!parser.TryParse(out HelpInfo info))
            {
                parser.PrintCommonErrorMessage();
                return;
            }

            Console.WriteLine("SteganographyApp Help\n");

            foreach (string message in info.GetMessagesFor(HelpItemSet.Main))
            {
                Console.WriteLine("{0}\n", message);
            }
        }
    }
}