while (true)
{
    Terminal.Out("String: ");

    var str = Terminal.ReadLine() ?? string.Empty;

    Terminal.OutLine("Width: {0}", MonospaceWidth.Measure(str.ReplaceLineEndings(string.Empty)));
}
