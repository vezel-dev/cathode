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

    internal ChildProcessReader(StreamReader reader, int bufferSize, CancellationToken cancellationToken)
    {
        _pipe = new(new(pauseWriterThreshold: bufferSize, useSynchronizationContext: false));
        Stream = new SynchronizedStream(_pipe.Reader.AsStream());
        Encoding = reader.CurrentEncoding;
        TextReader = new SynchronizedTextReader(
            new StreamReader(Stream, Encoding, detectEncodingFromByteOrderMarks: false, ReadBufferSize));

        var readStream = reader.BaseStream;
        var writeStream = _pipe.Writer.AsStream();

        Completion = Task.Run(
            async () =>
            {
                var array = ArrayPool<byte>.Shared.Rent(ReadBufferSize);

                try
                {
                    int read;

                    while ((read = await readStream.ReadAsync(array, cancellationToken).ConfigureAwait(false)) != 0)
                        await writeStream.WriteAsync(array.AsMemory(..read), cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    // The user requested cancellation of the process. We need to pass a cancellation token and handle
                    // this case explicitly because, if the user is not actually reading the output we are buffering
                    // here, the WriteAsync() call above may end up blocking forever, meaning we would never loop around
                    // to the next ReadAsync() call that is expected to fail.
                }
                catch (IOException)
                {
                    // The child process either exited or closed the pipe. Either way, treat it as EOF.
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(array);

                    // Users of the Stream and TextReader properties might block forever if we do not signal completion
                    // on the write end of the pipe. We do not signal completion on the read end of the pipe since we
                    // want users to be able to read all buffered data after the process exits.
                    await _pipe.Writer.CompleteAsync().ConfigureAwait(false);
                }
            },
            CancellationToken.None);
    }
}
