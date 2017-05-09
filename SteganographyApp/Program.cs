using System;

using SteganographyAppCommon;

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
                InputArguments inputArgs = ArgumentParser.Parse(args);

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

                    switch(e)
                    {
                        case TransformationException t:
                            Console.WriteLine("This error often occurs as a result of an incorrect password when decrypting a file.");
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
        }

        static void PrintHelp()
        {
            Console.WriteLine("Help");
            Console.WriteLine("Arguments must be specified with and = sign and no spaces.");
            Console.WriteLine("\tExample arguments for encoding a file to a set of images: ");
            Console.WriteLine("\t\tdotnet .\\SteganographApp =-action=encode --images=001.png,002.png --input=FileToEncode.zip --password=Pass1234 --randomSeed=monkey --compress=true");
            Console.WriteLine("\tExample arguments for decoding data from a set of images to an output file.");
            Console.WriteLine("\t\tdotnet .\\SteganpgraphyApp --action=decode --images=001.png,002.png --output=DecodedOutputFile.zip --password=Pass1234 --randomSeed=monkey --compress=true\n");
            Console.WriteLine("\t--action or -a :: Specifies whether to 'encode' a file to a set of images or 'decode' a set of images to a file.");
            Console.WriteLine("\t\tValue must be either 'encode', 'decode', or 'clean'.");
            Console.WriteLine("\t\tClean specifies that all LSBs in the set of images will be overwritten with garbage values.");
            Console.WriteLine("\t--input or -i :: The path to the file to encode if 'encode' was specified in the action argument.");
            Console.WriteLine("\t--output or -o :: The path to the output file when 'decode' was specified in the action argument.");
            Console.WriteLine("\t--images or -im :: A comma delimited list of paths to images to be either encoded or decoded");
            Console.WriteLine("\t\tThe order of the images affects the encoding and decoding results.");
            Console.WriteLine("\t--passsword or -p :: The password to encrypt the input file when 'encode' was specified in the action argument.");
            Console.WriteLine("\t--printStack or -ps :: Specifies whether or not to print the full stack trace if an error occurs.");
            Console.WriteLine("\t\tValue must either be 'true' or 'false'");
            Console.WriteLine("\t--compress or -c :: Specifies whether or not to compress/decompress the encoded/decoded content.");
            Console.WriteLine("\t\tValue must be either 'true' or 'false'.");
            Console.WriteLine("");
        }
    }
}