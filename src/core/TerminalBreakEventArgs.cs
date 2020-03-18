namespace System
{
    public sealed class TerminalBreakEventArgs : EventArgs
    {
        public TerminalBreakKey Key { get; }

        public bool Cancel { get; set; }

        internal TerminalBreakEventArgs(TerminalBreakKey key)
        {
            Key = key;
        }
    }
}
