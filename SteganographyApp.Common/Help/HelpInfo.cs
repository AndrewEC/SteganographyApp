namespace SteganographyApp.Common
{
    using System.Collections.Generic;
    using System.Collections.Immutable;

    public enum HelpItemSet
    {
        Main,
        Calculator,
        Converter,
    }

    /// <summary>
    /// Class containing the results of parsing the help file and a utiilty
    /// method to retrieve help messages for a given set of arguments.
    /// </summary>
    public sealed class HelpInfo
    {
#pragma warning disable SA1009

        public static readonly ImmutableArray<string> MainHelpLabels = ImmutableArray.Create(
            new string[]
            {
                "AppDescription",
                "AppAction",
                "Input",
                "Output",
                "Images",
                "Password",
                "PrintStack",
                "EnableCompression",
                "ChunkSize",
                "RandomSeed",
                "EnableDummies",
                "EnableLogs",
            }
        );

        public static readonly ImmutableArray<string> CalculatorHelpLabels = ImmutableArray.Create(
            new string[]
            {
                "CalculatorDescription",
                "CalculatorAction",
                "Input",
                "Images",
                "Password",
                "EnableCompression",
                "ChunkSize",
                "EnableDummies",
                "RandomSeed",
                "EnableLogs",
            }
        );

        public static readonly ImmutableArray<string> ConverterHelpLabels = ImmutableArray.Create(
            new string[]
            {
                "ConverterDescription",
                "Images",
                "DeleteOriginals",
                "CompressionLevel",
                "EnableLogs",
            }
        );

#pragma warning restore SA1009

        private static readonly ImmutableDictionary<HelpItemSet, ImmutableArray<string>> HelpLabelMappings = new Dictionary<HelpItemSet, ImmutableArray<string>>
        {
            { HelpItemSet.Main, MainHelpLabels },
            { HelpItemSet.Calculator, CalculatorHelpLabels },
            { HelpItemSet.Converter, ConverterHelpLabels },
        }
        .ToImmutableDictionary();

        /// <summary>
        /// A dictionary containing the results of the help file parsing.
        /// This will include a key value representing the name of the help
        /// message or the parameter that it corresponds to and a value
        /// being the actual help message.
        /// </summary>
        private readonly ImmutableDictionary<string, string> helpItems;

        public HelpInfo(ImmutableDictionary<string, string> helpItems)
        {
            this.helpItems = helpItems;
        }

        /// <summary>
        /// Attempts to retrieve an of array of help messages that are associated with
        /// the provided array of names and return them in the order in which they
        /// were requested.
        /// <para>If a particular parameter name was not loaded from the help file then a
        /// standard message will be returned in place of the actual help message.</para>
        /// </summary>
        /// <param name="item">The HelpItemSet enum value indicating which help items should
        /// be returned.</param>
        /// <returns>An array of help messages whose order is based on the order of the names
        /// provided to lookup.</returns>
        public ImmutableArray<string> GetHelpMessagesFor(HelpItemSet itemSet)
        {
            var helpLabels = HelpLabelMappings[itemSet];
            var messages = new string[helpLabels.Length];
            for (int i = 0; i < helpLabels.Length; i++)
            {
                if (!helpItems.ContainsKey(helpLabels[i]))
                {
                    messages[i] = $"No help information configured for {helpLabels[i]}.\n";
                    continue;
                }
                messages[i] = helpItems[helpLabels[i]];
            }
            return ImmutableArray.Create(messages);
        }
    }
}