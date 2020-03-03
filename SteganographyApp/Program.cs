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
                switch (e){
                    case ArgumentParseException e1:
                    case ArgumentValueException e2:
                        Console.WriteLine("An error ocurred during execution: \n\t{0}", e.Message);
                        break;
                    default:
                        #if DEBUG
                        if (arguments.PrintStack)
                        {
                            Console.WriteLine("Message Trace: ");
                            Console.WriteLine(e.StackTrace);
                        }
                        #endif

                        #if !DEBUG
                        Console.WriteLine("The action could not be completed. This can often indicate incorrect random seeds, passwords, images, etc.");
                        #endif
                        break;
                }
            }

            Console.WriteLine("");
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