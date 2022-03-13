namespace Cathode.Text.Control;

[Flags]
public enum MouseEvents
{
    None = 0b00,
    Movement = 0b01,
    Buttons = 0b10,
    All = Movement | Buttons,
}
