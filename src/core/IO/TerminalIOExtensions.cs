// SPDX-License-Identifier: 0BSD

namespace Vezel.Cathode.IO;

public static class TerminalIOExtensions
{
    public static int Read(this TerminalReader reader, scoped Span<byte> value)
    {
        Check.Null(reader);

        var count = 0;

        while (count < value.Length)
        {
            var ret = reader.ReadPartial(value[count..]);

            // EOF?
            if (ret == 0)
                break;

            count += ret;
        }

        return count;
    }

    public static ValueTask<int> ReadAsync(
        this TerminalReader reader, Memory<byte> value, CancellationToken cancellationToken = default)
    {
        Check.Null(reader);

        return ReadAsync();

        [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder<>))]
        async ValueTask<int> ReadAsync()
        {
            var count = 0;

            while (count < value.Length)
            {
                var ret = await reader.ReadPartialAsync(value[count..], cancellationToken).ConfigureAwait(false);

                // EOF?
                if (ret == 0)
                    break;

                count += ret;
            }

            return count;
        }
    }

    public static string? ReadLine(this TerminalReader reader)
    {
        Check.Null(reader);

        return reader.TextReader.ReadLine();
    }

    public static ValueTask<string?> ReadLineAsync(
        this TerminalReader reader, CancellationToken cancellationToken = default)
    {
        Check.Null(reader);

        return reader.TextReader.ReadLineAsync(cancellationToken);
    }

    public static void Write<T>(this TerminalWriter writer, T value)
    {
        writer.Write((value?.ToString()).AsSpan());
    }

    public static void Write(this TerminalWriter writer, scoped ReadOnlySpan<byte> value)
    {
        Check.Null(writer);

        for (var count = 0; count < value.Length; count += writer.WritePartial(value[count..]))
        {
        }
    }

    public static void Write(this TerminalWriter writer, scoped Span<byte> value)
    {
        writer.Write((ReadOnlySpan<byte>)value);
    }

    public static void Write(this TerminalWriter writer, ReadOnlyMemory<byte> value)
    {
        writer.Write(value.Span);
    }

    public static void Write(this TerminalWriter writer, Memory<byte> value)
    {
        writer.Write((ReadOnlyMemory<byte>)value);
    }

    public static void Write(this TerminalWriter writer, byte[]? value)
    {
        writer.Write(value.AsSpan());
    }

    public static void Write(this TerminalWriter writer, scoped ReadOnlySpan<char> value)
    {
        var encoding = Terminal.Encoding;
        var len = encoding.GetByteCount(value);
        var array = ArrayPool<byte>.Shared.Rent(len);

        try
        {
            var span = array.AsSpan(..len);

            _ = encoding.GetBytes(value, span);

            writer.Write(span);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(array);
        }
    }

    public static void Write(this TerminalWriter writer, scoped Span<char> value)
    {
        writer.Write((ReadOnlySpan<char>)value);
    }

    public static void Write(this TerminalWriter writer, ReadOnlyMemory<char> value)
    {
        writer.Write(value.Span);
    }

    public static void Write(this TerminalWriter writer, Memory<char> value)
    {
        writer.Write((ReadOnlyMemory<char>)value);
    }

    public static void Write(this TerminalWriter writer, char[]? value)
    {
        writer.Write(value.AsMemory());
    }

    public static void Write(this TerminalWriter writer, string? value)
    {
        writer.Write(value.AsMemory());
    }

    public static ValueTask WriteAsync<T>(
        this TerminalWriter writer, T value, CancellationToken cancellationToken = default)
    {
        return writer.WriteAsync((value?.ToString()).AsMemory(), cancellationToken);
    }

    public static ValueTask WriteAsync(
        this TerminalWriter writer, ReadOnlyMemory<byte> value, CancellationToken cancellationToken = default)
    {
        Check.Null(writer);

        return WriteAsync();

        [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder))]
        async ValueTask WriteAsync()
        {
            for (var count = 0;
                count < value.Length;
                count += await writer.WritePartialAsync(value[count..], cancellationToken).ConfigureAwait(false))
            {
            }
        }
    }

    public static ValueTask WriteAsync(
        this TerminalWriter writer, Memory<byte> value, CancellationToken cancellationToken = default)
    {
        return writer.WriteAsync((ReadOnlyMemory<byte>)value, cancellationToken);
    }

    public static ValueTask WriteAsync(
        this TerminalWriter writer, byte[]? value, CancellationToken cancellationToken = default)
    {
        return writer.WriteAsync(value.AsMemory(), cancellationToken);
    }

    public static ValueTask WriteAsync(
        this TerminalWriter writer, ReadOnlyMemory<char> value, CancellationToken cancellationToken = default)
    {
        Check.Null(writer);

        return WriteAsync();

        [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder))]
        async ValueTask WriteAsync()
        {
            var encoding = Terminal.Encoding;
            var len = encoding.GetByteCount(value.Span);
            var array = ArrayPool<byte>.Shared.Rent(len);

            try
            {
                var mem = array.AsMemory(..len);

                _ = encoding.GetBytes(value.Span, mem.Span);

                await writer.WriteAsync(mem, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(array);
            }
        }
    }

    public static ValueTask WriteAsync(
        this TerminalWriter writer, Memory<char> value, CancellationToken cancellationToken = default)
    {
        return writer.WriteAsync((ReadOnlyMemory<char>)value, cancellationToken);
    }

    public static ValueTask WriteAsync(
        this TerminalWriter writer, char[]? value, CancellationToken cancellationToken = default)
    {
        return writer.WriteAsync(value.AsMemory(), cancellationToken);
    }

    public static ValueTask WriteAsync(
        this TerminalWriter writer, string? value, CancellationToken cancellationToken = default)
    {
        return writer.WriteAsync(value.AsMemory(), cancellationToken);
    }

    public static void WriteLine(this TerminalWriter writer)
    {
        writer.WriteLine(string.Empty);
    }

    public static void WriteLine<T>(this TerminalWriter writer, T value)
    {
        writer.Write(value?.ToString() + Environment.NewLine);
    }

    public static ValueTask WriteLineAsync(this TerminalWriter writer, CancellationToken cancellationToken = default)
    {
        return writer.WriteLineAsync(string.Empty, cancellationToken);
    }

    public static ValueTask WriteLineAsync<T>(
        this TerminalWriter writer, T value, CancellationToken cancellationToken = default)
    {
        return writer.WriteAsync(value?.ToString() + Environment.NewLine, cancellationToken);
    }
}
