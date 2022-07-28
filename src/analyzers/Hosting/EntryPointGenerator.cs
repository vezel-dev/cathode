namespace Vezel.Cathode.Analyzers.Hosting;

[Generator(LanguageNames.CSharp)]
public sealed class EntryPointGenerator : ISourceGenerator
{
    private sealed class ProgramTypeSyntaxReceiver : ISyntaxContextReceiver
    {
        public ImmutableArray<INamedTypeSymbol> ProgramSymbols { get; private set; } =
            ImmutableArray<INamedTypeSymbol>.Empty;

        private INamedTypeSymbol? _interface;

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            var sema = context.SemanticModel;

            _interface ??= sema.Compilation.GetTypeByMetadataName("Vezel.Cathode.Hosting.IProgram");

            if (context.Node is TypeDeclarationSyntax type)
                if (sema.GetDeclaredSymbol(type) is INamedTypeSymbol sym)
                    if (sym.AllInterfaces.Any(sym => sym.Equals(_interface, SymbolEqualityComparer.Default)))
                        ProgramSymbols = ProgramSymbols.Add(sym);
        }
    }

    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new ProgramTypeSyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        var syms = ((ProgramTypeSyntaxReceiver)context.SyntaxContextReceiver!).ProgramSymbols;

        // Is the project using the terminal hosting APIs?
        if (syms.IsEmpty)
            return;

        if (syms.Length != 1)
            foreach (var program in syms)
                foreach (var loc in program.Locations)
                    context.ReportDiagnostic(
                        Diagnostic.Create(DiagnosticDescriptors.AvoidMultipleProgramTypes, loc));

        if (context.Compilation.GetEntryPoint(context.CancellationToken) is IMethodSymbol entry)
        {
            foreach (var loc in entry.Locations)
                context.ReportDiagnostic(
                    Diagnostic.Create(DiagnosticDescriptors.AvoidImplementingEntryPoint, loc, entry));

            return;
        }

        var name = syms[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        context.AddSource(
            "GeneratedProgram.g.cs",
            $$"""
            [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute]
            static class GeneratedProgram
            {
                static global::System.Threading.Tasks.Task Main(string[] args)
                {
                    return global::Vezel.Cathode.Hosting.ProgramHost.RunAsync<{{name}}>(args);
                }
            }
            """);
    }
}
