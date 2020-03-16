namespace System.IO
{
    public sealed class TerminalOutputStream : Stream
    {
        public ITerminalWriter Writer { get; }

        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        internal TerminalOutputStream(ITerminalWriter writer)
        {
            Writer = writer;
        }

        // We intentionally do not implement Dispose.

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(ReadOnlySpan<byte> buffer)
        {
            Writer.Write(buffer);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _ = buffer ?? throw new ArgumentNullException(nameof(buffer));
            _ = offset >= 0 ? true : throw new ArgumentOutOfRangeException(nameof(offset));
            _ = count >= 0 ? true : throw new ArgumentOutOfRangeException(nameof(count));
            _ = offset + count <= buffer.Length ? true : throw new ArgumentException();

            Write(buffer.AsSpan(offset, count));
        }

        public override unsafe void WriteByte(byte value)
        {
            Write(new ReadOnlySpan<byte>(&value, 1));
        }
    }
}
