namespace System.Text.Control;

public static class ControlConstants
{
    public const string ESC = "\x1b";

    public const string CSI = $"{ESC}[";

    public const string OSC = $"{ESC}]";

    public const string ST = $"{ESC}\\";

    public const string BEL = "\a";

    public const string LF = "\n";

    public const string CR = "\r";

    public const string SP = " ";
}
