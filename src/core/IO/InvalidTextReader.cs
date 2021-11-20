namespace System.IO;

sealed class InvalidTextReader : TextReader
{
    [SuppressMessage("ApiDesign", "RS0030")]
    public override int Read()
    {
        throw new InvalidOperationException(
            $"Intermixing of {typeof(Console)} and {typeof(Terminal)} is not supported.");
    }
}
