namespace System.IO
{
    public interface ITerminalWriter : ITerminalHandle
    {
        TerminalOutputStream Stream { get; }

        void Write(ReadOnlySpan<byte> data);
    }
}
