namespace SteganographyApp.Common.Tests;

using System.Collections.Generic;

using NUnit.Framework;

using SteganographyApp.Common.Injection.Proxies;

[TestFixture]
public class ProgressTrackerTests
{
    private const int DesiredWriteCount = 10;
    private const int DesiredWriteLineCount = 1;
    private const string Message = "testing";
    private const string CompleteMessage = "testing complete";

    private MockWriter mockWriter;

    private ProgressTracker tracker;

    [SetUp]
    public void BeforeEach()
    {
        mockWriter = new MockWriter();
        tracker = new ProgressTracker(DesiredWriteCount, Message, CompleteMessage, mockWriter);
    }

    [Test]
    public void TestProgressTrackerIsInvokedCorrectNumberOfTimes()
    {
        ExecuteUpdates(tracker);

        Assert.That(mockWriter.WriteValues, Has.Count.EqualTo(DesiredWriteCount));
        Assert.That(mockWriter.WriteLineValues, Has.Count.EqualTo(DesiredWriteLineCount));
    }

    [Test]
    public void TestProgressTrackerInvokesWriteAndWriteLineWithCorrectPercentageIndicators()
    {
        ExecuteUpdates(tracker);

        int index = 0;
        int[] percents = [0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100];
        var currentNode = mockWriter.WriteValues.First;
        while (currentNode != null)
        {
            Assert.That(currentNode.Value, Does.Contain(percents[index].ToString()));
            index++;
            currentNode = currentNode.Next;
        }
    }

    private static void ExecuteUpdates(ProgressTracker tracker)
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