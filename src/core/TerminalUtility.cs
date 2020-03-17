using System.Buffers;
using System.Text;

namespace System
{
    static class TerminalUtility
    {
        public delegate void EncodedAction(ReadOnlySpan<byte> data);

        public const int InvalidSize = -1;

        public static Encoding Encoding { get; } = Encoding.UTF8;

        public static void EncodeAndExecute(ReadOnlySpan<char> value, Encoding encoding, EncodedAction action)
        {
            var len = encoding.GetByteCount(value);
            var array = ArrayPool<byte>.Shared.Rent(len);

            try
            {
                var span = array.AsSpan(0, len);

                _ = encoding.GetBytes(value, span);

                action(span);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(array);
            }
        }
    }
}
