namespace Vezel.Cathode.Analyzers.Hosting;

[Generator]
public sealed class EntryPointGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterSourceOutput(
            context.CompilationProvider
                .Select(static (c, ct) => c.GetEntryPoint(ct))
                .Combine(
                    context.SyntaxProvider
                        .CreateSyntaxProvider(
                            static (node, _) => node is TypeDeclarationSyntax,
                            static (ctx, _) => (Node: (TypeDeclarationSyntax)ctx.Node, Model: ctx.SemanticModel))
                        .Combine(
                            context.MetadataReferencesProvider
                                .Combine(context.CompilationProvider)
                                .Select(static (tup, _) => tup.Right.GetAssemblyOrModuleSymbol(tup.Left))
                                .Where(static sym => sym is IAssemblySymbol { Name: "Vezel.Cathode.Hosting" })
                                .Collect()
                                .Select(static (arr, _) =>
                                    ((IAssemblySymbol?)arr.FirstOrDefault())?.GetTypeByMetadataName(
                                        "Vezel.Cathode.Hosting.IProgram")))
                        .Select(static (tup, ct) =>
                            (Interface: tup.Right, Candidate: tup.Left.Model.GetDeclaredSymbol(tup.Left.Node, ct)))
                        .Where(static tup =>
                            tup is (not null, not null) &&
                            tup.Candidate.AllInterfaces.Any(iface =>
                                iface.Equals(tup.Interface, SymbolEqualityComparer.Default)))
                        .Select(static (tup, _) => tup.Candidate)
                        .Collect()),
            static (ctx, tup) =>
            {
                var syms = tup.Right;

                // Is the project using the terminal hosting APIs?
                if (syms.IsEmpty)
                    return;

                if (syms.Length != 1)
                    foreach (var sym in syms)
                        foreach (var loc in sym!.Locations)
                            ctx.ReportDiagnostic(
                                Diagnostic.Create(DiagnosticDescriptors.AvoidMultipleProgramTypes, loc));

                if (tup.Left is IMethodSymbol entry)
                {
                    foreach (var loc in entry.Locations)
                        ctx.ReportDiagnostic(
                            Diagnostic.Create(DiagnosticDescriptors.AvoidImplementingEntryPoint, loc, entry));

                    return;
                }

                var name = syms[0]!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

                ctx.AddSource(
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
            });
    }
}
