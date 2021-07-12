namespace SteganographyApp.Common
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Reflection;

    using SteganographyApp.Common.Injection;

    /// <summary>
    /// Utility class to parse help information from the help.prop file
    /// so it can be shared between the regular app and the calculator.
    /// </summary>
    public class HelpParser
    {
        /// <summary>
        /// When found at the beginning of a line it indicates a new help item
        /// entry.
        /// </summary>
        private static readonly string HelpItemIndicator = "~";

        /// <summary>
        /// Gets a value representing the last exception message to be caught.
        /// </summary>
        public string LastError { get; private set; }

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

        /// <summary>
        /// Attempts to parse the help file located at the fileName parameter value.
        /// <para> If successful it will have populated the <paramref name="info"/> dictionary with all the
        /// help messages and the parameters that they are associated with.</para>
        /// <para>If not successful it will exit early and set the value of the <see cref="LastError"/>
        /// property to a message identifying why the process failed.</para>
        /// </summary>
        /// <param name="info">The HelpInfo instance that will be set by the end of execution. If an exception
        /// ocurrs during execution then this will be set to null.</param>
        /// <param name="filename">The path to the help file to read from.</param>
        /// <returns>True if the help file was successfully parsed, otherwise false.</returns>
        public bool TryParseHelpFile(out HelpInfo info, string filename = "help.prop")
        {
            string assemblyPath = GetAssemblyPath();
            string helpFileLocation = $"{assemblyPath}\\{filename}";

            if (!Injector.Provide<IFileIOProxy>().IsExistingFile(helpFileLocation))
            {
                LastError = $"The help file named {helpFileLocation} could not be found.";
                info = new HelpInfo(null);
                return false;
            }

            try
            {
                info = new HelpInfo(ParseHelpItems(helpFileLocation));
            }
            catch (Exception e)
            {
                LastError = e.Message;
                info = new HelpInfo(null);
                return false;
            }

            return true;
        }

        private ImmutableDictionary<string, string> ParseHelpItems(string helpFileLocation)
        {
            var helpItems = new Dictionary<string, string>();
            string[] lines = Injector.Provide<IFileIOProxy>().ReadAllLines(helpFileLocation);
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith(HelpItemIndicator))
                {
                    helpItems[LineWithoutHelpItemIndicator(lines[i])] = ReadHelpItem(lines, i + 1);
                }
            }
            return helpItems.ToImmutableDictionary();
        }

        private string LineWithoutHelpItemIndicator(string line) => line.Substring(1);

        private string GetAssemblyPath()
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            int index = assemblyPath.LastIndexOf("\\");
            if (index == -1)
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
        /// <param name="startLine">The index to start reading the help message lines from.</param>
        /// <returns>A single string containing all the lines of the help message delimeted by line breaks.</returns>
        private string ReadHelpItem(string[] lines, int startLine)
        {
            string message = string.Empty;
            for (int i = startLine; i < lines.Length; i++)
            {
                if (lines[i].StartsWith(HelpItemIndicator))
                {
                    break;
                }
                else if (lines[i].Trim().Length == 0)
                {
                    continue;
                }
                if (i != startLine)
                {
                    message += "\n";
                }
                message += lines[i].Replace("\\t", "\t");
            }
            return message;
        }
    }
}