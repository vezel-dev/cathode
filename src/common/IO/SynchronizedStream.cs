using Vezel.Cathode.Diagnostics;
using Vezel.Cathode.Threading;

namespace Vezel.Cathode.IO;

[SuppressMessage("", "CA2213")]
internal sealed class SynchronizedStream : Stream
{
    // The stream returned by Stream.Synchronized has no async support, has some questionable semantics surrounding
    // BeginRead/EndRead and BeginWrite/EndWrite, and also lacks forwarding for newer method overloads. This class
    // addresses those issues.
    //
    // Note that Close, Dispose, and DisposeAsync are intentionally not forwarded as this class is meant to wrap streams
    // that are disposed by other means or are intended to live for the duration of the program.

    private sealed class AsyncOperation : IAsyncResult
    {
        public bool Read { get; }

        public IAsyncResult Result { get; }

        public bool Ended { get; set; }

        object? IAsyncResult.AsyncState => Result.AsyncState;

        WaitHandle IAsyncResult.AsyncWaitHandle => Result.AsyncWaitHandle;

        bool IAsyncResult.CompletedSynchronously => Result.CompletedSynchronously;

        bool IAsyncResult.IsCompleted => Result.IsCompleted;

        public AsyncOperation(bool read, IAsyncResult result)
        {
            Read = read;
            Result = result;
        }
    }

    public override bool CanRead => _stream.CanRead;

    public override bool CanWrite => _stream.CanWrite;

    public override bool CanSeek => _stream.CanSeek;

    public override bool CanTimeout => _stream.CanTimeout;

    public override long Length
    {
        get
        {
            using (_lock.Enter())
                return _stream.Length;
        }
    }

    public override long Position
    {
        get
        {
            using (_lock.Enter())
                return _stream.Position;
        }

        set
        {
            using (_lock.Enter())
                _stream.Position = value;
        }
    }

    public override int ReadTimeout
    {
        get => _stream.ReadTimeout;
        set => _stream.ReadTimeout = value;
    }

    public override int WriteTimeout
    {
        get => _stream.WriteTimeout;
        set => _stream.WriteTimeout = value;
    }

    private readonly SemaphoreSlim _lock = new(1, 1);

    private readonly Stream _stream;

    public SynchronizedStream(Stream stream)
    {
        _stream = stream;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        using (_lock.Enter())
            return _stream.Seek(offset, origin);
    }

    public override void SetLength(long value)
    {
        using (_lock.Enter())
            _stream.SetLength(value);
    }

    public override void CopyTo(Stream destination, int bufferSize)
    {
        using (_lock.Enter())
            _stream.CopyTo(destination, bufferSize);
    }

    public override async Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
    {
        using (await _lock.EnterAsync(cancellationToken).ConfigureAwait(false))
            await _stream.CopyToAsync(destination, bufferSize, cancellationToken).ConfigureAwait(false);
    }

    public override void Flush()
    {
        using (_lock.Enter())
            _stream.Flush();
    }

    public override async Task FlushAsync(CancellationToken cancellationToken)
    {
        using (await _lock.EnterAsync(cancellationToken).ConfigureAwait(false))
            await _stream.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        using (_lock.Enter())
            return _stream.Read(buffer, offset, count);
    }

    public override int Read(Span<byte> buffer)
    {
        using (_lock.Enter())
            return _stream.Read(buffer);
    }

    [SuppressMessage("", "CA1835")]
    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        using (await _lock.EnterAsync(cancellationToken).ConfigureAwait(false))
            return await _stream.ReadAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
    }

    [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder<>))]
    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        using (await _lock.EnterAsync(cancellationToken).ConfigureAwait(false))
            return await _stream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
    }

    public override int ReadByte()
    {
        using (_lock.Enter())
            return _stream.ReadByte();
    }

    public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state)
    {
        _lock.Wait();

        return new AsyncOperation(
            true,
            _stream.BeginRead(
                buffer,
                offset,
                count,
                ar =>
                {
                    _ = _lock.Release();

                    callback?.Invoke(ar);
                },
                state));
    }

    public override int EndRead(IAsyncResult asyncResult)
    {
        Check.Null(asyncResult);

        var op = asyncResult as AsyncOperation;

        Check.Argument(op?.Read == true, asyncResult);
        Check.Operation(!op.Ended);

        using (_lock.Enter())
        {
            op.Ended = true;

            return _stream.EndRead(op.Result);
        }
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        using (_lock.Enter())
            _stream.Write(buffer, offset, count);
    }

    public override void Write(ReadOnlySpan<byte> buffer)
    {
        using (_lock.Enter())
            _stream.Write(buffer);
    }

    [SuppressMessage("", "CA1835")]
    public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        using (await _lock.EnterAsync(cancellationToken).ConfigureAwait(false))
            await _stream.WriteAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
    }

    [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder))]
    public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        using (await _lock.EnterAsync(cancellationToken).ConfigureAwait(false))
            await _stream.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
    }

    public override void WriteByte(byte value)
    {
        using (_lock.Enter())
            _stream.WriteByte(value);
    }

    public override IAsyncResult BeginWrite(
        byte[] buffer, int offset, int count, AsyncCallback? callback, object? state)
    {
        _lock.Wait();

        return new AsyncOperation(
            false,
            _stream.BeginWrite(
                buffer,
                offset,
                count,
                ar =>
                {
                    _ = _lock.Release();

                    callback?.Invoke(ar);
                },
                state));
    }

    public override void EndWrite(IAsyncResult asyncResult)
    {
        Check.Null(asyncResult);

        var op = asyncResult as AsyncOperation;

        Check.Argument(op?.Read == false, asyncResult);
        Check.Operation(!op.Ended);

        using (_lock.Enter())
        {
            op.Ended = true;

            _stream.EndWrite(op.Result);
        }
    }
}
