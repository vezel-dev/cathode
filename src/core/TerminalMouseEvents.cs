namespace System;

[Flags]
public enum TerminalMouseEvents
{
    None = 0b00,
    Movement = 0b01,
    Buttons = 0b10,
    All = Movement | Buttons,
}
