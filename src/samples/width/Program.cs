while (true)
{
    Terminal.Out("String: ");

    if (Terminal.ReadLine() is not string str)
        break;

    Terminal.OutLine("Width: {0}", MonospaceWidth.Measure(str.ReplaceLineEndings(string.Empty)));
}
