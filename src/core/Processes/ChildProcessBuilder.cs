namespace Vezel.Cathode.Processes;

public sealed class ChildProcessBuilder
{
    public string FileName { get; private set; } = string.Empty;

    public ImmutableArray<string> Arguments { get; private set; } = ImmutableArray<string>.Empty;

    public ImmutableDictionary<string, string> Variables { get; private set; } =
        ImmutableDictionary<string, string>.Empty;

    public string WorkingDirectory { get; private set; } = string.Empty;

    public bool CreateWindow { get; private set; } = true;

    public ProcessWindowStyle WindowStyle { get; private set; }

    public bool RedirectStandardIn { get; private set; } = true;

    public bool RedirectStandardOut { get; private set; } = true;

    public bool RedirectStandardError { get; private set; } = true;

    public int StandardOutBufferSize { get; private set; }

    public int StandardErrorBufferSize { get; private set; }

    // The sad reality is that some programs use insane encodings even in today's world, so we do still need to expose
    // these properties for those pathological cases.

    public Encoding StandardInEncoding { get; private set; } = Terminal.Encoding;

    public Encoding StandardOutEncoding { get; private set; } = Terminal.Encoding;

    public Encoding StandardErrorEncoding { get; private set; } = Terminal.Encoding;

    public CancellationToken CancellationToken { get; private set; }

    public bool ThrowOnError { get; private set; } = true;

    ChildProcessBuilder Clone()
    {
        return new()
        {
            FileName = FileName,
            Arguments = Arguments,
            Variables = Variables,
            WorkingDirectory = WorkingDirectory,
            CreateWindow = CreateWindow,
            WindowStyle = WindowStyle,
            RedirectStandardIn = RedirectStandardIn,
            RedirectStandardOut = RedirectStandardOut,
            RedirectStandardError = RedirectStandardError,
            StandardOutBufferSize = StandardOutBufferSize,
            StandardErrorBufferSize = StandardErrorBufferSize,
            StandardInEncoding = StandardInEncoding,
            StandardOutEncoding = StandardOutEncoding,
            StandardErrorEncoding = StandardErrorEncoding,
            CancellationToken = CancellationToken,
            ThrowOnError = ThrowOnError,
        };
    }

    public ChildProcessBuilder WithFileName(string fileName)
    {
        ArgumentNullException.ThrowIfNull(fileName);

        var builder = Clone();

        builder.FileName = fileName;

        return builder;
    }

    public ChildProcessBuilder WithArguments(ImmutableArray<string> arguments)
    {
        if (arguments.IsDefault)
            arguments = ImmutableArray<string>.Empty;
        else
            _ = arguments.All(a => a != null) ? true : throw new ArgumentException(null, nameof(arguments));

        var builder = Clone();

        builder.Arguments = arguments;

        return builder;
    }

    public ChildProcessBuilder WithArguments(params string[] arguments)
    {
        return WithArguments(arguments.ToImmutableArray());
    }

    public ChildProcessBuilder AddArgument(string argument)
    {
        ArgumentNullException.ThrowIfNull(argument);

        var builder = Clone();

        builder.Arguments = Arguments.Add(argument);

        return builder;
    }

    public ChildProcessBuilder AddArguments(params string[] arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        _ = arguments.All(a => a != null) ? true : throw new ArgumentException(null, nameof(arguments));

        var builder = Clone();

        builder.Arguments = Arguments.AddRange(arguments);

        return builder;
    }

    public ChildProcessBuilder WithVariables(ImmutableDictionary<string, string> environment)
    {
        ArgumentNullException.ThrowIfNull(environment);
        _ = environment.All(kvp => kvp.Key != null && kvp.Value != null) ?
            true : throw new ArgumentException(null, nameof(environment));

        var builder = Clone();

        builder.Variables = environment;

        return builder;
    }

    public ChildProcessBuilder WithVariables(params (string, string)[] environment)
    {
        return WithVariables(environment.ToImmutableDictionary(t => t.Item1, t => t.Item2));
    }

    public ChildProcessBuilder AddVariable(string name, string value)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(value);

        var builder = Clone();

        builder.Variables = Variables.Add(name, value);

        return builder;
    }

    public ChildProcessBuilder SetVariable(string name, string value)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(value);

        var builder = Clone();

        builder.Variables = Variables.SetItem(name, value);

        return builder;
    }

    public ChildProcessBuilder RemoveVariable(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        var builder = Clone();

        builder.Variables = Variables.Remove(name);

        return builder;
    }

    public ChildProcessBuilder ClearVariables()
    {
        return WithVariables();
    }

    public ChildProcessBuilder WithWorkingDirectory(string workingDirectory)
    {
        ArgumentNullException.ThrowIfNull(workingDirectory);

        var builder = Clone();

        builder.WorkingDirectory = workingDirectory;

        return builder;
    }

    public ChildProcessBuilder WithCreateWindow(bool createWindow)
    {
        var builder = Clone();

        builder.CreateWindow = createWindow;

        return builder;
    }

    public ChildProcessBuilder WithWindowStyle(ProcessWindowStyle windowStyle)
    {
        _ = Enum.IsDefined(windowStyle) ? true : throw new ArgumentOutOfRangeException(nameof(windowStyle));

        var builder = Clone();

        builder.WindowStyle = windowStyle;

        return builder;
    }

    public ChildProcessBuilder WithRedirections(bool standardIn, bool standardOut, bool standardError)
    {
        var builder = Clone();

        builder.RedirectStandardIn = standardIn;
        builder.RedirectStandardOut = standardOut;
        builder.RedirectStandardError = standardError;

        return builder;
    }

    public ChildProcessBuilder WithRedirections(bool allStreams)
    {
        return WithRedirections(allStreams, allStreams, allStreams);
    }

    public ChildProcessBuilder WithBufferSizes(int standardOut, int standardError)
    {
        _ = standardOut >= 0 ? true : throw new ArgumentOutOfRangeException(nameof(standardOut));
        _ = standardError >= 0 ? true : throw new ArgumentOutOfRangeException(nameof(standardError));

        var builder = Clone();

        builder.StandardOutBufferSize = standardOut;
        builder.StandardErrorBufferSize = standardError;

        return builder;
    }

    public ChildProcessBuilder WithBufferSizes(int allStreams)
    {
        return WithBufferSizes(allStreams, allStreams);
    }

    public ChildProcessBuilder WithEncodings(Encoding? standardIn, Encoding? standardOut, Encoding? standardError)
    {
        var builder = Clone();

        builder.StandardInEncoding = standardIn ?? Terminal.Encoding;
        builder.StandardOutEncoding = standardOut ?? Terminal.Encoding;
        builder.StandardErrorEncoding = standardError ?? Terminal.Encoding;

        return builder;
    }

    public ChildProcessBuilder WithEncodings(Encoding? allStreams)
    {
        return WithEncodings(allStreams, allStreams, allStreams);
    }

    public ChildProcessBuilder WithCancellationToken(CancellationToken cancellationToken)
    {
        var builder = Clone();

        builder.CancellationToken = cancellationToken;

        return builder;
    }

    public ChildProcessBuilder WithThrowOnError(bool throwOnError)
    {
        var builder = Clone();

        builder.ThrowOnError = throwOnError;

        return builder;
    }

    public ChildProcess Run()
    {
        return new(this);
    }
}
