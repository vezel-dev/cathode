namespace System.Processes;

public sealed class ChildProcessReader
{
    // This buffer size is arbitrary and only affects performance.
    const int ReadBufferSize = 4096;

    public Stream Stream { get; }

    public Encoding Encoding { get; }

    public TextReader TextReader { get; }

    internal Task Completion { get; }

    readonly Pipe _pipe;

    internal ChildProcessReader(StreamReader reader, int bufferSize)
    {
        _pipe = new(new(pauseWriterThreshold: bufferSize, useSynchronizationContext: false));
        Stream = _pipe.Reader.AsStream();
        Encoding = reader.CurrentEncoding;
        TextReader = new StreamReader(Stream, Encoding, false);

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

                await _pipe.Writer.CompleteAsync().ConfigureAwait(false);
            }
        });
    }
}
