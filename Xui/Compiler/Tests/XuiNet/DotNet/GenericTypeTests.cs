using System.Reflection.Metadata;
using Xui.Compiler.XuiNet;
using Xui.Compiler.XuiNet.CodeFlow;
using Xui.Compiler.XuiNet.DotNet;
using Xui.Compiler.XuiNet.DotNet.Signatures;
using Xunit;

namespace Xui.Compiler.Tests.XuiNet.DotNet;

public class GenericTypeTests
{
    [Fact]
    public void ConstructedIdentitiesAreStructuralAndRetainTheirDefinition()
    {
        const string source = """
            public class Box<T> { public T Value; public Box(T value) { Value = value; } }
            public static class Program {
                public static int Main() { var a = new Box<int>(1); var b = new Box<string>("x"); return a.Value; }
            }
            """;
        using var stream = new MemoryStream(Frontend.Roslyn.Compile(source));
        using var assembly = AssemblyCompiler.Read(stream);
        var module = assembly.ManifestModule;
        var specifications = module.TypeSpecifications.ToArray();
        var intSpec = Assert.Single(specifications, handle => module.GetTypeSpecification(handle).Read().Identity?.Arguments?.Values[0].Code == SignatureTypeCode.Int32);
        var first = module.GetTypeSpecification(intSpec).Read();
        var again = module.GetTypeSpecification(intSpec).Read();
        var text = Assert.Single(specifications, handle => module.GetTypeSpecification(handle).Read().Identity?.Arguments?.Values[0].Code == SignatureTypeCode.String);
        var other = module.GetTypeSpecification(text).Read();
        Assert.Equal(first, again);
        Assert.Equal(first.GetHashCode(), again.GetHashCode());
        Assert.NotEqual(first, other);
        Assert.Equal(first.Identity!.Value.GenericDefinition, other.Identity!.Value.GenericDefinition);
        Assert.False(first.Identity.Value.IsAssignableTo(other.Identity.Value));
        var tree = new StackTree();
        Assert.Same(tree.Empty.Push(StackEntry.From(first)), tree.Empty.Push(StackEntry.From(again)));

        var declaration = module.GetType((TypeDefinitionHandle)first.Identity.Value.Handle);
        var parameter = module.GetGenericParameter(Assert.Single(declaration.GenericParameters));
        Assert.Equal("T", parameter.Name);
        Assert.Equal(0, parameter.Index);
        var field = module.GetField(Assert.Single(declaration.Fields));
        Assert.Equal(SignatureTypeCode.GenericTypeParameter, SignatureReader.Field(module, field.Signature).Code);
        Assert.Equal(SignatureTypeCode.Int32, SignatureReader.Field(module, field.Signature, first.Identity.Value.Arguments).Code);
        Assert.Equal(SignatureTypeCode.String, SignatureReader.Field(module, field.Signature, other.Identity.Value.Arguments).Code);

        var constructors = module.MemberReferences.Where(handle => module.GetMemberReference(handle).Name == ".ctor" &&
            module.GetMemberReference(handle).Parent.Kind == HandleKind.TypeSpecification).ToArray();
        Assert.Equal(2, constructors.Length);
        var context = MethodIdentity.FromDefinition(module, module.EntryPoint).DeclaringType;
        var intConstructor = MemberResolver.Method(module, constructors[0], context);
        var stringConstructor = MemberResolver.Method(module, constructors[1], context);
        Assert.Equal(intConstructor.Handle, stringConstructor.Handle);
        Assert.NotEqual(intConstructor, stringConstructor);
        foreach (var constructor in new[] { intConstructor, stringConstructor })
            Assert.All(Analyzer.Analyze(module, constructor).Instructions, node => Assert.NotNull(node.After));
    }
}
