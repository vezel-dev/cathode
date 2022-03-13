namespace Cathode;

public class TestVirtualTerminal : VirtualTerminal
{
    public override sealed event Action<TerminalSize>? Resized;

    public override sealed event Action<TerminalSignalContext>? Signaled;

    public override sealed event Action? Resumed;

    public override sealed TerminalReader StandardIn { get; }

    public override sealed TerminalWriter StandardOut { get; }

    public override sealed TerminalWriter StandardError { get; }

    public override sealed TerminalReader TerminalIn { get; }

    public override sealed TerminalWriter TerminalOut { get; }

    public override sealed bool IsRawMode => _raw;

    public override sealed TerminalSize Size => _size;

    bool _raw;

    TerminalSize _size = new(24, 80);

    public TestVirtualTerminal()
    {
        var input = new TestTerminalReader();
        var output = new TestTerminalWriter();

        // TODO: Split standard error into a separate writer that also pipes to terminal out.

        StandardIn = input;
        StandardOut = output;
        StandardError = output;
        TerminalIn = input;
        TerminalOut = output;
    }

    public override sealed void GenerateSignal(TerminalSignal signal)
    {
        Signaled?.Invoke(new(signal));
    }

    public void GenerateResume()
    {
        Resumed?.Invoke();
    }

    public override sealed void EnableRawMode()
    {
        _raw = true;

        // TODO: Flush the input buffer.
    }

    public override sealed void DisableRawMode()
    {
        _raw = false;

        // TODO: Flush the input buffer.
    }

    public void SetSize(TerminalSize size)
    {
        if (_size == size)
            return;

        _size = size;

        Resized?.Invoke(size);
    }
}
