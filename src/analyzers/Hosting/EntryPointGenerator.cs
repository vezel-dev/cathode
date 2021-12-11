using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace System.Analyzers.Hosting;

[Generator(LanguageNames.CSharp)]
public sealed class EntryPointGenerator : ISourceGenerator
{
    sealed class ProgramClassSyntaxReceiver : ISyntaxContextReceiver
    {
        public ImmutableArray<INamedTypeSymbol> ProgramSymbols { get; private set; } =
            ImmutableArray<INamedTypeSymbol>.Empty;

        INamedTypeSymbol? _interface;

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            var sema = context.SemanticModel;

            _interface ??= sema.Compilation.GetTypeByMetadataName("System.Hosting.IProgram");

            if (context.Node is ClassDeclarationSyntax cls)
            {
                if (sema.GetDeclaredSymbol(cls) is INamedTypeSymbol sym)
                {
                    if (sym.AllInterfaces.Any(sym => sym.Equals(_interface, SymbolEqualityComparer.Default)))
                    {
                        ProgramSymbols = ProgramSymbols.Add(sym);
                    }
                }
            }
        }
    }

    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new ProgramClassSyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        var syms = ((ProgramClassSyntaxReceiver)context.SyntaxContextReceiver!).ProgramSymbols;

        // Is the project using the terminal hosting APIs?
        if (syms.IsEmpty)
            return;

        if (syms.Length != 1)
            foreach (var program in syms)
                foreach (var loc in program.Locations)
                    context.ReportDiagnostic(
                        Diagnostic.Create(DiagnosticDescriptors.AvoidMultipleProgramClasses, loc));

        var entry = context.Compilation.GetEntryPoint(context.CancellationToken);

        if (entry != null)
            foreach (var loc in entry.Locations)
                context.ReportDiagnostic(
                    Diagnostic.Create(DiagnosticDescriptors.AvoidSpecifyingEntryPoint, loc, entry));

        if (entry == null)
            context.AddSource("Program.g.cs", $@"
[global::System.Runtime.CompilerServices.CompilerGenerated]
static class GeneratedProgram
{{
    static async global::System.Threading.Tasks.Task Main(string[] args)
    {{
        await global::System.Hosting.ProgramHost.RunAsync<{syms[0].Name}>(args);
    }}
}}");
    }
}
