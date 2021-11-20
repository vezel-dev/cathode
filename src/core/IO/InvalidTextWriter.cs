namespace System.IO;

sealed class InvalidTextWriter : TextWriter
{
    [SuppressMessage("ApiDesign", "RS0030")]
    public override Encoding Encoding => Console.OutputEncoding;

    [SuppressMessage("ApiDesign", "RS0030")]
    public override void Write(char value)
    {
        throw new InvalidOperationException(
            $"Intermixing of {typeof(Console)} and {typeof(Terminal)} is not supported.");
    }
}
