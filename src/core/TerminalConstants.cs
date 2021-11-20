namespace System;

static class TerminalConstants
{
    public const string ESC = "\x1b";

    public const string CSI = ESC + "[";

    public const string OSC = ESC + "]";

    public const string BEL = "\a";
}
