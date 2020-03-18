namespace System.IO
{
    sealed class InvalidTextReader : TextReader
    {
        public override int Read()
        {
            throw new InvalidOperationException(
                $"Intermixing of {typeof(Console)} and {typeof(Terminal)} is not supported.");
        }
    }
}
