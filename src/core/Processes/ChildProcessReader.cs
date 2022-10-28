namespace Vezel.Cathode.Processes;

public sealed class ChildProcessReader
{
    // This buffer size is arbitrary and only affects performance.
    private const int ReadBufferSize = 4096;

    public Stream Stream { get; }

    public Encoding Encoding { get; }

    public TextReader TextReader { get; }

    internal Task Completion { get; }

    private readonly Pipe _pipe;

    internal ChildProcessReader(StreamReader reader, int bufferSize)
    {
        _pipe = new(new(pauseWriterThreshold: bufferSize, useSynchronizationContext: false));
        Stream = new SynchronizedStream(_pipe.Reader.AsStream());
        Encoding = reader.CurrentEncoding;
        TextReader = new SynchronizedTextReader(new StreamReader(Stream, Encoding, false, ReadBufferSize));

        var readStream = reader.BaseStream;
        var writeStream = _pipe.Writer.AsStream();

        Completion = Task.Run(async () =>
        {
            var array = ArrayPool<byte>.Shared.Rent(ReadBufferSize);

            try
            {
                int read;

                // TODO: Review exceptions that can be thrown here.
                while ((read = await readStream.ReadAsync(array).ConfigureAwait(false)) != 0)
                    await writeStream.WriteAsync(array.AsMemory(..read)).ConfigureAwait(false);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(array);

                // Users of the Stream and TextReader properties might block forever if we do not signal completion on
                // the write end of the pipe. We do not signal completion on the read end of the pipe since we want
                // users to be able to read all buffered data after the process exits.
                await _pipe.Writer.CompleteAsync().ConfigureAwait(false);
            }
        });
    }
}
