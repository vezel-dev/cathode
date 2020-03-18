namespace System
{
    static class TerminalConstants
    {
        public const string Escape = "\x1b";

        public const string CSI = Escape + "[";

        public const string OSC = Escape + "]";
    }
}
