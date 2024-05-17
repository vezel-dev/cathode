// SPDX-License-Identifier: 0BSD

namespace Vezel.Cathode.Text.Control;

public static class ControlConstants
{
    public const char NUL = '\0';

    public const char SOH = '\x01';

    public const char STX = '\x02';

    public const char ETX = '\x03';

    public const char EOT = '\x04';

    public const char ENQ = '\x05';

    public const char ACK = '\x06';

    public const char BEL = '\a';

    public const char BS = '\b';

    public const char HT = '\t';

    public const char LF = '\n';

    public const char VT = '\v';

    public const char FF = '\f';

    public const char CR = '\r';

    public const char SO = '\x0e';

    public const char SI = '\x0f';

    public const char DLE = '\x10';

    public const char DC1 = '\x11';

    public const char DC2 = '\x12';

    public const char DC3 = '\x13';

    public const char DC4 = '\x14';

    public const char NAK = '\x15';

    public const char SYN = '\x16';

    public const char ETB = '\x17';

    public const char CAN = '\x18';

    public const char EM = '\x19';

    public const char SUB = '\x1a';

    public const char ESC = '\x1b';

    public const char FS = '\x1c';

    public const char GS = '\x1d';

    public const char RS = '\x1e';

    public const char US = '\x1f';

    public const char SP = ' ';

    public const char DEL = '\x7f';

    // We cannot use char constants in constant interpolated strings...

    public const string DCS = "\x1bP";

    public const string SOS = "\x1bX";

    public const string CSI = "\x1b[";

    public const string OSC = "\x1b]";

    public const string PM = "\x1b^";

    public const string APC = "\x1b_";

    public const string ST = "\x1b\\";
}
