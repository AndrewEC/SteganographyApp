namespace SteganographyApp.Common.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;

    public static class MockInput
    {
        public static Queue<ConsoleKeyInfo> CreateInputQueue(ImmutableList<(char KeyChar, ConsoleKey Key)> inputMapping)
        {
            var queue = new Queue<ConsoleKeyInfo>();
            for (int i = 0; i < inputMapping.Count; i++)
            {
                (char keyChar, ConsoleKey key) = inputMapping[i];
                if (keyChar >= 65)
                {
                    queue.Enqueue(new ConsoleKeyInfo(keyChar, key, true, false, false));
                    continue;
                }
                queue.Enqueue(new ConsoleKeyInfo(keyChar, key, false, false, false));
            }
            return queue;
        }
    }
}