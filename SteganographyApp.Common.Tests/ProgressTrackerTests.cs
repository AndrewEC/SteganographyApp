namespace SteganographyApp.Common.Tests
{
    using System.Collections.Generic;

    using NUnit.Framework;

    using SteganographyApp.Common.Injection;

    [TestFixture]
    public class ProgressTrackerTests : FixtureWithTestObjects
    {
        private static readonly int DesiredWriteCount = 10;
        private static readonly int DesiredWriteLineCount = 1;
        private static readonly string Message = "testing";
        private static readonly string CompleteMessage = "testing complete";

        private MockWriter mockWriter;

        private ProgressTracker tracker;

        [SetUp]
        public void BeforeEach()
        {
            mockWriter = new MockWriter();
            Injector.UseInstance<IConsoleWriter>(mockWriter);
            tracker = new ProgressTracker(DesiredWriteCount, Message, CompleteMessage);
        }

        [Test]
        public void TestProgressTrackerIsInvokedCorrectNumberOfTimes()
        {
            ExecuteUpdates(tracker);

            Assert.AreEqual(DesiredWriteCount, mockWriter.WriteValues.Count);
            Assert.AreEqual(DesiredWriteLineCount, mockWriter.WriteLineValues.Count);
        }

        [Test]
        public void TestProgressTrackerInvokesWriteAndWriteLineWithCorrectPercentageIndicators()
        {
            ExecuteUpdates(tracker);

            int index = 0;
            int[] percents = new int[] { 0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100 };
            var currentNode = mockWriter.WriteValues.First;
            while (currentNode != null)
            {
                Assert.IsTrue(currentNode.Value.Contains(percents[index].ToString()));
                index++;
                currentNode = currentNode.Next;
            }
        }

        private void ExecuteUpdates(ProgressTracker tracker)
        {
            tracker.Display();
            for (int i = 0; i < DesiredWriteCount; i++)
            {
                tracker.UpdateAndDisplayProgress();
            }
        }
    }

    internal class MockWriter : IConsoleWriter
    {
        public LinkedList<string> WriteValues { get; private set; } = new LinkedList<string>();

        public LinkedList<string> WriteLineValues { get; private set; } = new LinkedList<string>();

        public void Write(string line)
        {
            WriteValues.AddLast(line);
        }

        public void WriteLine(string line)
        {
            WriteLineValues.AddLast(line);
        }
    }
}