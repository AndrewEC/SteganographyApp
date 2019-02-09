using SteganographyApp.Common;
using SteganographyApp.Common.Data;
using SteganographyApp.Common.Help;
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
            if(!parser.TryParse(args, out InputArguments arguments))
            {
                Console.WriteLine("\nAn exception occured while parsing arguments:\n\t{0}", parser.LastError.Message);
                if (parser.LastError.InnerException != null)
                {
                    Console.WriteLine("And was caused by:\n\t{0}", parser.LastError.InnerException.Message);
                }
                Console.WriteLine("\nRun the program with --help to get more information.");
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

        static void PrintHelp()
        {

            var parser = new HelpParser();
            if (!parser.TryParse(out HelpInfo info))
            {
                Console.WriteLine("An error occurred while parsing the help file: {0}", parser.LastError);
                Console.WriteLine("Check that the help.prop file is in the same directory as the application and try again.");
                Console.WriteLine();
                return;
            }

            Console.WriteLine("SteganographyApp Help\n");

            foreach (string message in info.GetMessagesFor("AppDescription", "AppAction", "Input", "Output", "Images", "Password",
                "PrintStack", "Compress", "ChunkSize", "RandomSeed", "Dummies"))
            {
                Console.WriteLine("{0}\n", message);
            }
        }
    }
}