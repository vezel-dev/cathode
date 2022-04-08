namespace Vezel.Cathode.Processes;

public sealed class ChildProcessWriter
{
    // This buffer size is arbitrary and only affects performance.
    const int WriteBufferSize = 4096;

    public Stream Stream { get; }

    public Encoding Encoding { get; }

    public TextWriter TextWriter { get; }

    internal ChildProcessWriter(StreamWriter writer)
    {
        Stream = new SynchronizedStream(writer.BaseStream);
        Encoding = writer.Encoding;
        TextWriter = new SynchronizedTextWriter(new StreamWriter(Stream, Encoding, WriteBufferSize)
        {
            AutoFlush = true,
        });
    }
}
