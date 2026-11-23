using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xui.CompileTime.Discovery;
using Xui.CompileTime.Emission;
using Xui.CompileTime.SyntaxWalking;
using Xui.CompileTime.TypeMapping;
using Xui.GPU.IR;

namespace Xui.CompileTime;

[Generator]
public class ShaderProgramGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var shaderPrograms = context.SyntaxProvider.ForAttributeWithMetadataName(
            "Xui.GPU.Shaders.Attributes.ShaderProgramAttribute",
            predicate: (node, _) => node is StructDeclarationSyntax,
            transform: (ctx, _) => ShaderProgramCollector.Extract(ctx)
        ).Where(sp => sp is not null);

        context.RegisterSourceOutput(shaderPrograms, (spc, info) =>
        {
            try
            {
                var source = Generate(info!);
                var fileName = $"{info!.TypeName}.Generated.g.cs";
                spc.AddSource(fileName, source);
            }
            catch (Exception ex)
            {
                // Report generator errors as diagnostics rather than crashing the build
                spc.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor(
                        "XUIGPU001",
                        "Shader generation failed",
                        "Failed to generate shader for '{0}': {1}",
                        "Xui.CompileTime",
                        DiagnosticSeverity.Error,
                        isEnabledByDefault: true),
                    Location.None,
                    info!.TypeName,
                    ex.Message));
            }
        });
    }

    private static string Generate(ShaderProgramInfo info)
    {
        var typeMapper = new ShaderTypeMapper();
        var module = new IrShaderModule { Name = info.TypeName };

        // Build vertex stage
        if (info.VertexStage is { } vs)
        {
            var inputIr = typeMapper.MapVertexInputType(vs.InputType);
            var outputIr = typeMapper.MapStructType(vs.OutputType);
            var bindingsIr = vs.BindingsType != null ? typeMapper.MapBindingsType(vs.BindingsType) : null;

            // Add struct declarations to module
            module.Structs.Add(new IrStructDecl(inputIr));
            module.Structs.Add(new IrStructDecl(outputIr));
            if (bindingsIr != null)
                module.Structs.Add(new IrStructDecl(bindingsIr));

            // Walk Execute method body
            var walker = new ExecuteMethodWalker(
                vs.SemanticModel, typeMapper,
                "input", inputIr,
                "bindings", bindingsIr ?? inputIr, // fallback shouldn't happen
                outputIr);

            var body = walker.Walk(vs.ExecuteMethodSyntax);

            module.VertexStage = new IrVertexStage
            {
                Name = $"{info.TypeName}VertexShader",
                InputType = inputIr,
                OutputType = outputIr,
                BindingsType = bindingsIr,
                Body = body
            };
        }

        // Build fragment stage
        if (info.FragmentStage is { } fs)
        {
            var inputIr = typeMapper.MapStructType(fs.InputType);
            var outputIr = typeMapper.MapFragmentOutputType(fs.OutputType);
            var bindingsIr = fs.BindingsType != null ? typeMapper.MapBindingsType(fs.BindingsType) : null;

            // Add output struct if not already added (input/bindings may already be in module from VS)
            if (!module.Structs.Any(s => s.Type.Name == outputIr.Name))
                module.Structs.Add(new IrStructDecl(outputIr));

            var walker = new ExecuteMethodWalker(
                fs.SemanticModel, typeMapper,
                "input", inputIr,
                "bindings", bindingsIr ?? inputIr,
                outputIr);

            var body = walker.Walk(fs.ExecuteMethodSyntax);

            module.FragmentStage = new IrFragmentStage
            {
                Name = $"{info.TypeName}FragmentShader",
                BindingsType = bindingsIr,
                InputType = inputIr,
                OutputType = outputIr,
                Body = body
            };
        }

        return ShaderSourceEmitter.Emit(info.Namespace, info.TypeName, module);
    }
}
