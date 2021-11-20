namespace System;

static class TerminalUtility
{
    public static Thread StartThread(string name, ThreadStart body)
    {
        var thread = new Thread(body)
        {
            IsBackground = true,
            Name = name,
        };

        thread.Start();

        return thread;
    }
}
