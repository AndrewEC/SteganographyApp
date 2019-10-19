using Microsoft.VisualStudio.TestTools.UnitTesting;
using SteganographyApp.Common.Test;
using Moq;
using System.Collections.Generic;

namespace SteganographyApp.Common.Tests
{

    [TestClass]
    public class ProgressTrackerTests
    {

        [TestMethod]
        public void TestProgressTrackerInvokesWriteWithProperValues()
        {
            var mockWriter = new Mock<IWriter>();
            var lines = new List<string>();
            var newLines = new List<string>();
            mockWriter.Setup(writer => writer.Write(It.IsAny<string>())).Callback<string>(line => lines.Add(line));
            mockWriter.Setup(writer => writer.WriteLine(It.IsAny<string>())).Callback<string>(line => newLines.Add(line));

            var tracker = new ProgressTracker(10, "testing", "testing complete", mockWriter.Object);
            tracker.Display();
            for (int i = 0; i < 10; i++)
            {
                tracker.TickAndDisplay();
            }

            mockWriter.Verify(writer => writer.Write(It.IsAny<string>()), Times.Exactly(10));
            mockWriter.Verify(writer => writer.WriteLine(It.IsAny<string>()), Times.Once());

            int[] percents = new int[] { 0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100 };
            for (int i = 0; i < lines.Count; i++)
            {
                Assert.IsTrue(lines[i].Contains(percents[i].ToString()));
            }
        }

    }

}