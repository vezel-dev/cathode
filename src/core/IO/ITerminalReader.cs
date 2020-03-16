namespace System.IO
{
    public interface ITerminalReader : ITerminalHandle
    {
        TerminalInputStream Stream { get; }

        int Read(Span<byte> data);
    }
}
