namespace Vezel.Cathode.Analyzers;

internal static class DiagnosticDescriptors
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false)]
    private sealed class DiagnosticAttribute : Attribute
    {
        public string Title { get; }

        public string Message { get; }

        public DiagnosticSeverity Severity { get; }

        public DiagnosticAttribute(string title, string message, DiagnosticSeverity severity)
        {
            Title = title;
            Message = message;
            Severity = severity;
        }
    }

    [Diagnostic(
        "Avoid multiple types implementing 'IProgram'",
        "There must only be one type implementing 'IProgram' within an assembly",
        DiagnosticSeverity.Error)]
    public static DiagnosticDescriptor AvoidMultipleProgramTypes { get; private set; } = null!;

    [Diagnostic(
        "Avoid manually implementing an entry point",
        "Manually-implemented entry point method '{0}' conflicts with the generated entry point",
        DiagnosticSeverity.Error)]
    public static DiagnosticDescriptor AvoidImplementingEntryPoint { get; private set; } = null!;

    static DiagnosticDescriptors()
    {
        var id = 1000;

        foreach (var p in typeof(DiagnosticDescriptors)
            .GetProperties(BindingFlags.Static | BindingFlags.Public)
            .Where(p => p.PropertyType == typeof(DiagnosticDescriptor))
            .OrderBy(p => p.MetadataToken))
        {
            var attr = p.GetCustomAttribute<DiagnosticAttribute>();

            p.SetValue(
                null,
                new DiagnosticDescriptor(
                    $"CATH{id}", attr.Title, attr.Message, "Vezel.Cathode", attr.Severity, true));

            id++;
        }
    }
}
