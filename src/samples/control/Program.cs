// SPDX-License-Identifier: 0BSD

static async ValueTask RunAsync(string message, Action action)
{
    var tcs = new TaskCompletionSource();

    await OutLineAsync($"Trying to {message} in non-control context...");

    _ = ThreadPool.UnsafeQueueUserWorkItem(
        _ =>
        {
            try
            {
                action();

                tcs.SetResult();
            }
            catch (InvalidOperationException e)
            {
                tcs.SetException(e);
            }
        },
        state: null);

    try
    {
        await tcs.Task;
    }
    catch (InvalidOperationException e)
    {
        await OutLineAsync($"Caught exception: {e.Message}");
    }
}

await OutLineAsync("Acquiring terminal control and running a few tests...");

using (var control = Control.Acquire())
{
    await OutLineAsync("Trying to output text in control context...");

    await RunAsync("output text", () => OutLine("This should throw."));
    await RunAsync("read a line", () => ReadLine());
    await RunAsync("switch to raw mode", EnableRawMode);
}

await OutLineAsync("Done.");
