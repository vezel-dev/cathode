// SPDX-License-Identifier: 0BSD

namespace Vezel.Cathode.Processes;

public sealed class ChildProcessBuilder
{
    public string FileName { get; private set; } = string.Empty;

    public ImmutableArray<string> Arguments { get; private set; } = [];

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

    private ChildProcessBuilder Clone()
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
        Check.Null(fileName);

        var builder = Clone();

        builder.FileName = fileName;

        return builder;
    }

    public ChildProcessBuilder WithArguments(params string[] arguments)
    {
        return WithArguments(arguments.AsEnumerable());
    }

    public ChildProcessBuilder WithArguments(IEnumerable<string> arguments)
    {
        Check.Null(arguments);
        Check.All(arguments, static arg => arg != null);

        var builder = Clone();

        builder.Arguments = arguments.ToImmutableArray();

        return builder;
    }

    public ChildProcessBuilder SetArgument(int index, string argument)
    {
        Check.Null(argument);

        var builder = Clone();

        builder.Arguments = Arguments.SetItem(index, argument);

        return builder;
    }

    public ChildProcessBuilder InsertArgument(int index, string argument)
    {
        Check.Null(argument);

        var builder = Clone();

        builder.Arguments = Arguments.Insert(index, argument);

        return builder;
    }

    public ChildProcessBuilder InsertArguments(int index, params string[] arguments)
    {
        return InsertArguments(index, arguments.AsEnumerable());
    }

    public ChildProcessBuilder InsertArguments(int index, IEnumerable<string> arguments)
    {
        Check.Null(arguments);
        Check.All(arguments, static arg => arg != null);

        var builder = Clone();

        builder.Arguments = Arguments.InsertRange(index, arguments);

        return builder;
    }

    public ChildProcessBuilder AddArgument(string argument)
    {
        Check.Null(argument);

        var builder = Clone();

        builder.Arguments = Arguments.Add(argument);

        return builder;
    }

    public ChildProcessBuilder AddArguments(params string[] arguments)
    {
        return AddArguments(arguments.AsEnumerable());
    }

    public ChildProcessBuilder AddArguments(IEnumerable<string> arguments)
    {
        Check.Null(arguments);
        Check.All(arguments, static arg => arg != null);

        var builder = Clone();

        builder.Arguments = Arguments.AddRange(arguments);

        return builder;
    }

    public ChildProcessBuilder RemoveArgument(int index)
    {
        var builder = Clone();

        builder.Arguments = Arguments.RemoveAt(index);

        return builder;
    }

    public ChildProcessBuilder RemoveArguments(int index, int count)
    {
        var builder = Clone();

        builder.Arguments = Arguments.RemoveRange(index, count);

        return builder;
    }

    public ChildProcessBuilder ClearArguments()
    {
        return WithArguments();
    }

    public ChildProcessBuilder WithVariables(params (string Name, string Value)[] variables)
    {
        return WithVariables(variables.Select(tup => KeyValuePair.Create(tup.Name, tup.Value)));
    }

    public ChildProcessBuilder WithVariables(IEnumerable<KeyValuePair<string, string>> variables)
    {
        Check.Null(variables);
        Check.All(variables, static kvp => kvp.Value != null);

        var builder = Clone();

        builder.Variables = variables.ToImmutableDictionary();

        return builder;
    }

    public ChildProcessBuilder SetVariable(string name, string value)
    {
        Check.Null(value);

        var builder = Clone();

        builder.Variables = Variables.SetItem(name, value);

        return builder;
    }

    public ChildProcessBuilder SetVariables(params (string Name, string Value)[] variables)
    {
        return SetVariables(variables.Select(tup => KeyValuePair.Create(tup.Name, tup.Value)));
    }

    public ChildProcessBuilder SetVariables(IEnumerable<KeyValuePair<string, string>> variables)
    {
        Check.Null(variables);
        Check.All(variables, static kvp => kvp.Value != null);

        var builder = Clone();

        builder.Variables = Variables.SetItems(variables);

        return builder;
    }

    public ChildProcessBuilder AddVariable(string name, string value)
    {
        Check.Null(value);

        var builder = Clone();

        builder.Variables = Variables.Add(name, value);

        return builder;
    }

    public ChildProcessBuilder AddVariables(params (string Name, string Value)[] variables)
    {
        return AddVariables(variables.Select(tup => KeyValuePair.Create(tup.Name, tup.Value)));
    }

    public ChildProcessBuilder AddVariables(IEnumerable<KeyValuePair<string, string>> variables)
    {
        Check.Null(variables);
        Check.All(variables, static kvp => kvp.Value != null);

        var builder = Clone();

        builder.Variables = Variables.AddRange(variables);

        return builder;
    }

    public ChildProcessBuilder RemoveVariable(string name)
    {
        var builder = Clone();

        builder.Variables = Variables.Remove(name);

        return builder;
    }

    public ChildProcessBuilder RemoveVariables(params string[] names)
    {
        return RemoveVariables(names.AsEnumerable());
    }

    public ChildProcessBuilder RemoveVariables(IEnumerable<string> names)
    {
        Check.Null(names);
        Check.All(names, static name => name != null);

        var builder = Clone();

        builder.Variables = Variables.RemoveRange(names);

        return builder;
    }

    public ChildProcessBuilder ClearVariables()
    {
        return WithVariables();
    }

    public ChildProcessBuilder WithWorkingDirectory(string workingDirectory)
    {
        Check.Null(workingDirectory);

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
        Check.Enum(windowStyle);

        var builder = Clone();

        builder.WindowStyle = windowStyle;

        return builder;
    }

    public ChildProcessBuilder WithRedirections(bool allStreams)
    {
        return WithRedirections(allStreams, allStreams, allStreams);
    }

    public ChildProcessBuilder WithRedirections(bool standardIn, bool standardOut, bool standardError)
    {
        var builder = Clone();

        builder.RedirectStandardIn = standardIn;
        builder.RedirectStandardOut = standardOut;
        builder.RedirectStandardError = standardError;

        return builder;
    }

    public ChildProcessBuilder WithBufferSizes(int allStreams)
    {
        return WithBufferSizes(allStreams, allStreams);
    }

    public ChildProcessBuilder WithBufferSizes(int standardOut, int standardError)
    {
        Check.Range(standardOut >= 0, standardOut);
        Check.Range(standardError >= 0, standardError);

        var builder = Clone();

        builder.StandardOutBufferSize = standardOut;
        builder.StandardErrorBufferSize = standardError;

        return builder;
    }

    public ChildProcessBuilder WithEncodings(Encoding? allStreams)
    {
        return WithEncodings(allStreams, allStreams, allStreams);
    }

    public ChildProcessBuilder WithEncodings(Encoding? standardIn, Encoding? standardOut, Encoding? standardError)
    {
        var builder = Clone();

        builder.StandardInEncoding = standardIn ?? Terminal.Encoding;
        builder.StandardOutEncoding = standardOut ?? Terminal.Encoding;
        builder.StandardErrorEncoding = standardError ?? Terminal.Encoding;

        return builder;
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
