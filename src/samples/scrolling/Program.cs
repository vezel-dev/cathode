// SPDX-License-Identifier: 0BSD

await OutAsync(
    new ControlBuilder()
        .SetScreenBuffer(ScreenBuffer.Alternate)
        .MoveCursorTo(0, 0)
        .SetScrollMargin(2, Terminal.Size.Height));

try
{
    await OutAsync(
        new ControlBuilder()
            .SetDecorations(intense: true)
            .PrintLine("The last string entered will be displayed here.")
            .ResetAttributes()
            .PrintLine(new string('-', Terminal.Size.Width)));

    var rng = new Random();

    [SuppressMessage("", "CA5394")]
    byte PickRandom()
    {
        return (byte)rng.Next(byte.MinValue, byte.MaxValue + 1);
    }

    while (true)
    {
        await OutAsync("Input: ");

        if (await ReadLineAsync() is not string str)
            break;

        await OutAsync(
            new ControlBuilder()
                .SaveCursorState()
                .MoveCursorTo(0, 0)
                .ClearLine()
                .SetForegroundColor(Color.FromArgb(byte.MaxValue, PickRandom(), PickRandom(), PickRandom()))
                .Print(str.ReplaceLineEndings(string.Empty))
                .ResetAttributes()
                .RestoreCursorState());
    }
}
finally
{
    await OutAsync(
        new ControlBuilder()
            .SetScrollMargin(0, Terminal.Size.Height)
            .SetScreenBuffer(ScreenBuffer.Main));
}
