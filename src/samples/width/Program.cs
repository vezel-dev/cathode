// SPDX-License-Identifier: 0BSD

while (true)
{
    await OutAsync("String: ");

    if (await ReadLineAsync() is not string str)
        break;

    await OutLineAsync($"Width: {MonospaceWidth.Measure(str.ReplaceLineEndings(string.Empty))}");
}
