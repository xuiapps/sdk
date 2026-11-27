using Microsoft.CodeAnalysis;
using System.Reflection.Metadata;
using Xui.Compiler.XuiNet;
using Xui.Compiler.XuiNet.DotNet;
using Xui.Compiler.XuiNet.DotNet.Signatures;
using Xunit;

namespace Xui.Compiler.Tests.XuiNet.DotNet;

public class PointerSignatureTests
{
    [Fact]
    public void ResolvesArrayConstructorAlongsideUnmanagedPointerOverload()
    {
        using var stream = new MemoryStream(Frontend.Roslyn.Compile("""
            public static class Sample {
                public static int Count(byte[] values) => new System.ReadOnlySpan<byte>(values).Length;
            }
            """, OutputKind.DynamicallyLinkedLibrary));
        using var assembly = AssemblyCompiler.Read(stream);
        using var framework = new Frontend.Framework();
        _ = new Xui.Compiler.XuiNet.Compilation([assembly, .. framework.Assemblies]);
        var module = assembly.ManifestModule;
        var owner = new TypeIdentity(module, Assert.Single(module.Types, h => module.GetType(h).Name == "Sample"));
        var reference = Assert.Single(module.MemberReferences, h => module.GetMemberReference(h).Name == ".ctor" &&
            module.GetMemberReference(h).Parent.Kind == HandleKind.TypeSpecification);
        var resolved = MemberResolver.Method(module, reference, owner);
        var target = resolved.DeclaringType.Module;
        var signature = SignatureReader.Method(target, target.GetMethod(resolved.Handle).Signature, resolved.DeclaringType.Arguments);
        Assert.Equal(SignatureType.Vector(new(SignatureTypeCode.Byte)), Assert.Single(signature.Parameters));

        var pointerConstructor = target.GetType((TypeDefinitionHandle)resolved.DeclaringType.Handle).Methods
            .Where(h => target.GetMethod(h).Name == ".ctor")
            .Select(h => SignatureReader.Method(target, target.GetMethod(h).Signature, resolved.DeclaringType.Arguments))
            .Single(m => m.Parameters.Length == 2 && m.Parameters[0].Code == SignatureTypeCode.Pointer);
        Assert.Equal(SignatureTypeCode.Void, pointerConstructor.Parameters[0].Pointer!.Element.Code);
        Assert.Equal("Void*", pointerConstructor.Parameters[0].ToString());
        Assert.NotEqual(signature.Parameters[0], pointerConstructor.Parameters[0]);
    }
}
