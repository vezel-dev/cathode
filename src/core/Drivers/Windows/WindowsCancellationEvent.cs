using Windows.Win32.Foundation;
using static Windows.Win32.WindowsPInvoke;

namespace System.Drivers.Windows;

sealed class WindowsCancellationEvent
{
    readonly WindowsTerminalDriver _driver;

    readonly ManualResetEvent _event = new(false);

    public WindowsCancellationEvent(WindowsTerminalDriver driver)
    {
        _driver = driver;
    }

    public unsafe void PollWithCancellation(
        SafeHandle handle,
        Func<WindowsTerminalDriver, SafeHandle, bool> predicate,
        CancellationToken cancellationToken)
    {
        static void CancellationCallback(object? state)
        {
            _ = ((WindowsCancellationEvent)state!)._event.Set();
        }

        // The event handle must come first since, in a tie, we prefer cancellation.
        Span<HANDLE> handles = stackalloc HANDLE[]
        {
            (HANDLE)_event.SafeWaitHandle.DangerousGetHandle(),
            (HANDLE)handle.DangerousGetHandle(),
        };

        var canceled = false;

        using (var registration = cancellationToken.UnsafeRegister(CancellationCallback, this))
        {
            while (true)
            {
                var ret = WaitForMultipleObjects(handles, false, unchecked((uint)Timeout.Infinite));

                // Were we canceled?
                if (ret == WAIT_OBJECT_0)
                {
                    canceled = true;

                    break;
                }

                if (ret == WAIT_OBJECT_0 + 1 && predicate(_driver, handle))
                    break;
            }
        }

        if (canceled)
        {
            _ = _event.Reset();

            throw new OperationCanceledException(cancellationToken);
        }
    }
}
