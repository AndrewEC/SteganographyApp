using System.Collections.Immutable;

using NUnit.Framework;

namespace SteganographyApp.Common.Tests
{
    [TestFixture]
    public class HelpParserTests : FixtureWithRealObjects
    {

        private readonly string PositiveTestPath = "TestAssets/positive-help.prop";
        private readonly string NegativeTestPath = "TestAssets/negative-help.prop";

        [Test]
        public void TestParseHelpAndGetMainHelpItems()
        {
            RunHelpParserTest(HelpItemSet.Main, HelpInfo.MainHelpLabels);
        }

        [Test]
        public void TestParseHelpAndGetCalculatorHelpItems()
        {
            RunHelpParserTest(HelpItemSet.Calculator, HelpInfo.CalculatorHelpLabels);
        }

        [Test]
        public void TestParseHelpAndGetConverterHelpItems()
        {
            RunHelpParserTest(HelpItemSet.Converter, HelpInfo.ConverterHelpLabels);
        }

        private void RunHelpParserTest(HelpItemSet itemSet, ImmutableArray<string> expected)
        {
            var parser = new HelpParser();
            Assert.IsTrue(parser.TryParseHelpFile(out HelpInfo info, PositiveTestPath));
            Assert.IsNull(parser.LastError);

            var messages = info.GetHelpMessagesFor(itemSet);
            Assert.AreEqual(expected.Length, messages.Length);

            foreach (string line in messages)
            {
                Assert.IsNotNull(line);
                Assert.AreNotEqual("", line.Trim());
                Assert.IsFalse(line.Contains("No help information configured for"));
            }
        }

        [Test]
        public void TestMissingFileReturnsFalse()
        {
            var parser = new HelpParser();
            Assert.IsFalse(parser.TryParseHelpFile(out HelpInfo info, "missing"));
            Assert.IsNotNull(parser.LastError);
        }

        [Test]
        public void TestHelpParserWithMissingPropertiesProducesErrorInHelpItems()
        {
            var parser = new HelpParser();
            Assert.IsTrue(parser.TryParseHelpFile(out HelpInfo info, NegativeTestPath));
            foreach(string line in info.GetHelpMessagesFor(HelpItemSet.Calculator))
            {
                Assert.IsTrue(line.Contains("No help information configured for"));
            }
        }

    }
}
