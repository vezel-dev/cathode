namespace System.Diagnostics;

public sealed class TerminalProcessBuilder
{
    public string FileName { get; private set; } = string.Empty;

    public ImmutableArray<string> Arguments { get; private set; } = ImmutableArray<string>.Empty;

    public ImmutableDictionary<string, string> Variables { get; private set; } =
        ImmutableDictionary<string, string>.Empty;

    public string WorkingDirectory { get; private set; } = string.Empty;

    public bool UseShell { get; private set; }

    public bool NoWindow { get; private set; }

    public ProcessWindowStyle WindowStyle { get; private set; }

    public bool RedirectStandardIn { get; private set; }

    public bool RedirectStandardOut { get; private set; }

    public bool RedirectStandardError { get; private set; }

    // The sad reality is that some programs use insane encodings even in today's world, so we do still need to expose
    // these properties for those pathological cases.

    public Encoding StandardInEncoding { get; private set; } = Terminal.Encoding;

    public Encoding StandardOutEncoding { get; private set; } = Terminal.Encoding;

    public Encoding StandardErrorEncoding { get; private set; } = Terminal.Encoding;

    TerminalProcessBuilder Clone()
    {
        return new()
        {
            FileName = FileName,
            Arguments = Arguments,
            Variables = Variables,
            WorkingDirectory = WorkingDirectory,
            UseShell = UseShell,
            NoWindow = NoWindow,
            WindowStyle = WindowStyle,
            RedirectStandardIn = RedirectStandardIn,
            RedirectStandardOut = RedirectStandardOut,
            RedirectStandardError = RedirectStandardError,
            StandardInEncoding = StandardInEncoding,
            StandardOutEncoding = StandardOutEncoding,
            StandardErrorEncoding = StandardErrorEncoding,
        };
    }

    public TerminalProcessBuilder WithFileName(string fileName)
    {
        ArgumentNullException.ThrowIfNull(fileName);

        var builder = Clone();

        builder.FileName = fileName;

        return builder;
    }

    public TerminalProcessBuilder WithArguments(ImmutableArray<string> arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        _ = arguments.All(a => a != null) ? true : throw new ArgumentException(null, nameof(arguments));

        var builder = Clone();

        builder.Arguments = arguments;

        return builder;
    }

    public TerminalProcessBuilder WithArguments(params string[] arguments)
    {
        return WithArguments(arguments.ToImmutableArray());
    }

    public TerminalProcessBuilder AddArgument(string argument)
    {
        ArgumentNullException.ThrowIfNull(argument);

        var builder = Clone();

        builder.Arguments = Arguments.Add(argument);

        return builder;
    }

    public TerminalProcessBuilder AddArguments(params string[] arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        _ = arguments.All(a => a != null) ? true : throw new ArgumentException(null, nameof(arguments));

        var builder = Clone();

        builder.Arguments = Arguments.AddRange(arguments);

        return builder;
    }

    public TerminalProcessBuilder WithVariables(ImmutableDictionary<string, string> environment)
    {
        ArgumentNullException.ThrowIfNull(environment);

        var builder = Clone();

        builder.Variables = environment;

        return builder;
    }

    public TerminalProcessBuilder WithVariables(params (string, string)[] environment)
    {
        return WithVariables(environment.ToImmutableDictionary(t => t.Item1, t => t.Item2));
    }

    public TerminalProcessBuilder AddVariable(string name, string value)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(value);

        var builder = Clone();

        builder.Variables = Variables.Add(name, value);

        return builder;
    }

    public TerminalProcessBuilder SetVariable(string name, string value)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(value);

        var builder = Clone();

        builder.Variables = Variables.SetItem(name, value);

        return builder;
    }

    public TerminalProcessBuilder RemoveVariable(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        var builder = Clone();

        builder.Variables = Variables.Remove(name);

        return builder;
    }

    public TerminalProcessBuilder ClearVariables()
    {
        return WithVariables();
    }

    public TerminalProcessBuilder WithWorkingDirectory(string workingDirectory)
    {
        ArgumentNullException.ThrowIfNull(workingDirectory);

        var builder = Clone();

        builder.WorkingDirectory = workingDirectory;

        return builder;
    }

    public TerminalProcessBuilder WithUseShell(bool useShell)
    {
        var builder = Clone();

        builder.UseShell = useShell;

        return builder;
    }

    public TerminalProcessBuilder WithNoWindow(bool noWindow)
    {
        var builder = Clone();

        builder.NoWindow = noWindow;

        return builder;
    }

    public TerminalProcessBuilder WithWindowStyle(ProcessWindowStyle windowStyle)
    {
        _ = Enum.IsDefined(windowStyle) ? true : throw new ArgumentOutOfRangeException(nameof(windowStyle));

        var builder = Clone();

        builder.WindowStyle = windowStyle;

        return builder;
    }

    public TerminalProcessBuilder WithRedirections(bool standardIn, bool standardOut, bool standardError)
    {
        var builder = Clone();

        builder.RedirectStandardIn = standardIn;
        builder.RedirectStandardOut = standardOut;
        builder.RedirectStandardError = standardError;

        return builder;
    }

    public TerminalProcessBuilder WithRedirections(bool allStreams)
    {
        return WithRedirections(allStreams, allStreams, allStreams);
    }

    public TerminalProcessBuilder WithEncodings(Encoding standardIn, Encoding standardOut, Encoding standardError)
    {
        var builder = Clone();

        builder.StandardInEncoding = standardIn;
        builder.StandardOutEncoding = standardOut;
        builder.StandardErrorEncoding = standardError;

        return builder;
    }

    public TerminalProcessBuilder WithEncodings(Encoding allStreams)
    {
        return WithEncodings(allStreams, allStreams, allStreams);
    }

    public TerminalProcess Start()
    {
        return TerminalProcess.Start(this);
    }
}
