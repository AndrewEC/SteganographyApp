using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace SteganographyApp.Common
{

    public enum HelpItemSet
    {
        Main,
        Calculator,
        Converter
    }

    /// <summary>
    /// Class containing the results of parsing the help file and a utiilty
    /// method to retrieve help messages for a given set of arguments.
    /// </summary>
    public class HelpInfo
    {
        /// <summary>
        /// A dictionary containing the results of the help file parsing.
        /// This will include a key value representing the name of the help
        /// message or the parameter that it corresponds to and a value 
        /// being the actual help message.
        /// </summary>
        public Dictionary<string, string> Results { get; set; }

        /// <summary>
        /// Conversion point that takes in an <see cref="HelpItemSet"/> and invokes the
        /// <see cref="GetMessagesFor(string[])"/> method with the appropriate list of
        /// headings to lookup for the given item set enum value.
        /// </summary>
        /// <param name="item">The HelpItemSet enum value indicating which help items should
        /// be loaded and displayed to the user.</param>
        /// <returns>The value retrieved from the <see cref="GetMessagesFor(string[])"/>
        /// invocation</returns>
        public string[] GetMessagesFor(HelpItemSet item)
        {
            string[] labels = new string[0];
            switch(item)
            {
                case HelpItemSet.Main:
                    labels = new string[]{"AppDescription", "AppAction", "Input", "Output", "Images", "Password",
                        "PrintStack", "EnableCompression", "ChunkSize", "RandomSeed", "EnableDummies"};
                    break;
                case HelpItemSet.Calculator:
                    labels = new string[] {"CalculatorDescription", "AppAction", "Input", "Images", "Password",
                        "EnableCompression", "ChunkSize", "RandomSeed", "EnableDummies"};
                    break;
                case HelpItemSet.Converter:
                    labels = new string[] {"ConverterDescription", "ConvertAction", "Images", "DeleteOriginals",
                        "CompressionLevel"};
                    break;
            }
            return GetMessagesFor(labels);
        }

        /// <summary>
        /// Attempts to retrieve an of array of help messages that are associated with
        /// the provided array of names and return them in the order in which they
        /// were requested.
        /// <para>If a particular parameter name was not loaded from the help file then a
        /// standard message will be returned in place of the actual help message.</para>
        /// </summary>
        /// <param name="names">The ordered list of parameter names to retrieve the help
        /// messages for.</param>
        /// <returns>An array of help messages whose order is based on the order of the names
        /// provided to lookup.</returns>
        public string[] GetMessagesFor(params string[] names)
        {
            string[] messages = new string[names.Length];
            for (int i = 0; i < names.Length; i++)
            {
                if (!Results.ContainsKey(names[i]))
                {
                    messages[i] = string.Format("No help information configured for {0}.\n", names[i]);
                    continue;
                }
                messages[i] = Results[names[i]];
            }
            return messages;
        }
    }

    /// <summary>
    /// Utility class to parse help information from the help.prop file
    /// so it can be shared between the regular app and the calculator.
    /// </summary>
    public class HelpParser
    {

        /// <summary>
        /// The last error that occured during the execution of TryParse.
        /// </summary>
        public string LastError { get; private set; }

        /// <summary>
        /// Attempts to parse the help file located at the fileName parameter value.
        /// <para> If successful it will have populated the <see cref="Results"/> dictionary with all the
        /// help messages and the parameters that they are associated with.</para>
        /// <para>If not successful it will exit early and set the value of the <see cref="LastError"/>
        /// property to a message identifying why the process failed.</para>
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public bool TryParse(out HelpInfo info, string fileName = "help.prop")
        {
            info = new HelpInfo();

            string assemblyPath = GetAssemblyPath();
            string location = string.Format("{0}\\{1}", assemblyPath, fileName);
            
            if (!File.Exists(location))
            {
                LastError = string.Format("The help file named {0} could not be found.", location);
                return false;
            }

            Dictionary<string, string> results = new Dictionary<string, string>();
            string[] lines = File.ReadAllLines(location);

            for(int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("~"))
                {
                    try
                    {
                        results[lines[i].Substring(1)] = ReadItem(lines, i + 1);
                    }
                    catch (Exception e)
                    {
                        LastError = e.Message;
                        return false;
                    }
                }
            }

            info.Results = results;
            return true;
        }

        private string GetAssemblyPath()
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            int index = assemblyPath.LastIndexOf("\\");
            if(index == -1)
            {
                index = assemblyPath.LastIndexOf("/");
            }
            return assemblyPath.Substring(0, index);
        }

        /// <summary>
        /// Attempts to read all the lines of text that make up the help message that is associated
        /// with a particular parameter.
        /// <para>This will read all the lines from the start index provided until it reaches the end of
        /// the file or finds a line starting with a tilde (~) representing the start of a new parameter
        /// help message.</para>
        /// </summary>
        /// <param name="lines">The array of lines loaded from the help file.</param>
        /// <param name="start">The index to start reading the help message lines from.</param>
        /// <returns>A single string containing all the lines of the help message delimeted by line breaks.</returns>
        private string ReadItem(string[] lines, int start)
        {
            string message = "";
            for(int i = start; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("~"))
                {
                    break;
                }
                else if (lines[i].Trim().Length == 0)
                {
                    continue;
                }
                if(i != start)
                {
                    message += "\n";
                }
                message += lines[i].Replace("\\t", "\t");
            }
            return message;
        }

        /// <summary>
        /// Utility to print out a common error message in the instance where there is an error parsing
        /// the help properties file.
        /// </summary>
        public void PrintCommonErrorMessage()
        {
            Console.WriteLine("An error occurred while parsing the help file: {0}", LastError);
            Console.WriteLine("Check that the help.prop file is in the same directory as the application and try again.");
            Console.WriteLine();
        }
    }
}
