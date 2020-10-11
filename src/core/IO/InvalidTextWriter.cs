using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace System.IO
{
    sealed class InvalidTextWriter : TextWriter
    {
        [SuppressMessage("ApiDesign", "RS0030", Justification = "Needed.")]
        public override Encoding Encoding => Console.OutputEncoding;

        public override void Write(char value)
        {
            throw new InvalidOperationException(
                $"Intermixing of {typeof(Console)} and {typeof(Terminal)} is not supported.");
        }
    }
}
