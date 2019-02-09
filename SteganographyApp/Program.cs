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
            if (Array.IndexOf(args, "--help") != -1)
            {
                PrintHelp();
                return;
            }

            try
            {
                InputArguments inputArgs = new ArgumentParser().Parse(args);

                try
                {
                    new EntryPoint(inputArgs).Start();
                }
                catch (Exception e)
                {
                    Console.WriteLine("An error occured during execution: ");
                    Console.WriteLine("\tException Message: {0}", e.Message);
                    if (inputArgs.PrintStack)
                    {
                        Console.WriteLine(e.StackTrace);
                    }

                    switch (e)
                    {
                        case TransformationException t:
                            Console.WriteLine("This error often occurs as a result of an incorrect password or incorrect dummy count when decrypting a file.");
                            break;
                        case ArgumentOutOfRangeException t:
                            Console.WriteLine("This error often occurs when decoding using a dummy count much larger than the count specified when encoding.");
                            break;
                    }
                }
            }
            catch (ArgumentParseException e)
            {
                Console.WriteLine("\nAn exception occured while parsing arguments:\n\t{0}", e.Message);
                if (e.InnerException != null)
                {
                    Console.WriteLine("And was caused by:\n\t{0}", e.InnerException.Message);
                }
                Console.WriteLine("\nRun the program with --help to get more information.");
            }

            Console.WriteLine("");
        }

        static void PrintHelp()
        {

            var parser = new HelpParser();
            if (!parser.TryParse())
            {
                Console.WriteLine("An error occurred while parsing the help file: {0}", parser.LastError);
                Console.WriteLine("Check that the help.prop file is in the same directory as the application and try again.");
                Console.WriteLine();
                return;
            }

            Console.WriteLine("SteganographyApp Help\n");

            foreach (string message in parser.GetMessagesFor("AppDescription", "AppAction", "Input", "Output", "Images", "Password",
                "PrintStack", "Compress", "ChunkSize", "RandomSeed", "Dummies"))
            {
                Console.WriteLine("{0}\n", message);
            }
        }
    }
}