using Microsoft.VisualStudio.TestTools.UnitTesting;
using SteganographyApp.Common.Test;
using Moq;
using System.Collections.Generic;

namespace SteganographyApp.Common.Tests
{

    class MockWriter : IWriter
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

    [TestClass]
    public class ProgressTrackerTests
    {

        private readonly int DesiredWriteCount = 10;
        private readonly int DesiredWriteLineCount = 1;
        private readonly string Message = "testing";
        private readonly string CompleteMessage = "testing complete";

        private MockWriter mockWriter;
        private ProgressTracker tracker;

        [TestInitialize]
        public void BeforeEach()
        {
            mockWriter = new MockWriter();
            tracker = new ProgressTracker(DesiredWriteCount, Message, CompleteMessage, mockWriter);
        }

        private void ExecuteUpdates(ProgressTracker tracker)
        {
            tracker.Display();
            for (int i = 0; i < DesiredWriteCount; i++)
            {
                tracker.UpdateAndDisplayProgress();
            }
        }

        [TestMethod]
        public void TestProgressTrackerIsInvokedCorrectNumberOfTimes()
        {
            ExecuteUpdates(tracker);

            Assert.AreEqual(DesiredWriteCount, mockWriter.WriteValues.Count);
            Assert.AreEqual(DesiredWriteLineCount, mockWriter.WriteLineValues.Count);
        }

        [TestMethod]
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

    }

}