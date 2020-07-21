using SteganographyApp.Common;
using SteganographyApp.Common.Arguments;
using System;

namespace SteganographyApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("\nSteganography App\n");
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

            try
            {
                new EntryPoint(arguments).Start();
            }
            catch (Exception e)
            {
                HandleException(e, arguments);
            }

            Console.WriteLine("");
        }

        private static void HandleException(Exception e, IInputArguments arguments)
        {
            switch (e){
                case ArgumentParseException e1:
                case ArgumentValueException e2:
                    Console.WriteLine("An error ocurred parsing the provided arguments: \n\t{0}", e.Message);
                    break;
                default:
                    if (arguments.PrintStack)
                    {
                        Console.WriteLine("Message Trace: ");
                        Console.WriteLine(e.StackTrace);
                    }

                    Console.WriteLine($"An error ocurred during execution: {e.Message}");
                    break;
            }
        }

        /// <summary>
        /// Performs some validation once all the user inputted values have been parsed and individually
        /// validated.
        /// </summary>
        private static string PostValidate(IInputArguments input)
        {
            if (!Checks.IsOneOf(input.EncodeOrDecode, ActionEnum.Clean, ActionEnum.Encode,
                ActionEnum.Decode))
            {
                return "The action specified must be one of: 'clean', 'encode', or 'decode'.";
            }
            if (input.EncodeOrDecode == ActionEnum.Encode && Checks.IsNullOrEmpty(input.FileToEncode))
            {
                return "Specified encode action but no file to encode was provided in arguments.";
            }
            else if (input.EncodeOrDecode == ActionEnum.Decode && Checks.IsNullOrEmpty(input.DecodedOutputFile))
            {
                return "Specified decode action but no file to decode was provided in arguments.";
            }
            else if ((input.EncodeOrDecode == ActionEnum.Encode || input.EncodeOrDecode == ActionEnum.Decode) && Checks.IsNullOrEmpty(input.CoverImages))
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
            if (!parser.TryParseHelpFile(out HelpInfo info))
            {
                parser.PrintCommonErrorMessage();
                return;
            }

            Console.WriteLine("SteganographyApp Help\n");

            foreach (string message in info.GetHelpMessagesFor(HelpItemSet.Main))
            {
                Console.WriteLine("{0}\n", message);
            }
        }
    }
}