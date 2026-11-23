using Microsoft.CodeAnalysis;
using System.Reflection.Metadata;
using Xui.Compiler.XuiNet;
using Xui.Compiler.XuiNet.DotNet.Signatures;
using Xunit;
using Model = Xui.Compiler.XuiNet.DotNet;

namespace Xui.Compiler.Tests.XuiNet.DotNet;

public class CompilationTests
{
    [Fact]
    public void ResolvesNestedTypesAndMemberSignaturesAcrossMetadataTables()
    {
        byte[] libraryImage = Frontend.Roslyn.Compile("""
            namespace Library;
            public class Container { public class Payload { } }
            public static class Api
            {
                public static Container.Payload Value;
                public static int Echo(int value) => value;
                public static Container.Payload Echo(Container.Payload value) => value;
            }
            """, OutputKind.DynamicallyLinkedLibrary, "Library");
        using var library = Read(libraryImage);
        using var caller = Read(Frontend.Roslyn.Compile("""
            public static class Caller
            {
                public static Library.Container.Payload Run() => Library.Api.Echo(Library.Api.Value);
            }
            """, OutputKind.DynamicallyLinkedLibrary, "Caller",
            additionalReferences: [MetadataReference.CreateFromImage(libraryImage)]));
        var compilation = new Xui.Compiler.XuiNet.Compilation(caller, library);
        var source = caller.ManifestModule;
        var target = library.ManifestModule;
        var context = new Model.TypeIdentity(source, Assert.Single(source.Types, h => source.GetType(h).Name == "Caller"));
        var expected = new Model.TypeIdentity(target, Assert.Single(target.Types, h => target.GetType(h).Name == "Payload"));
        var payloadReference = Assert.Single(source.TypeReferences, h => source.GetTypeReference(h).Name == "Payload");
        Assert.Equal(expected, compilation.ResolveType(source, payloadReference));

        var callReference = Assert.Single(source.MemberReferences, h => source.GetMemberReference(h).Name == "Echo");
        var method = Model.MemberResolver.Method(source, callReference, context);
        Assert.Same(target, method.DeclaringType.Module);
        var signature = SignatureReader.Method(target, target.GetMethod(method.Handle).Signature);
        Assert.Equal(expected, signature.Result.Identity);
        Assert.Equal(expected, Assert.Single(signature.Parameters).Identity);

        var fieldReference = Assert.Single(source.MemberReferences, h => source.GetMemberReference(h).Name == "Value");
        var field = Model.MemberResolver.Field(source, fieldReference, context);
        Assert.Same(target, field.DeclaringType.Module);
        Assert.Equal(expected, SignatureReader.Field(target, target.GetField(field.Handle).Signature).Identity);
    }

    [Fact]
    public void RejectsDuplicateAssemblyNamesWithoutPartiallyBindingInputs()
    {
        byte[] image = Frontend.Roslyn.Compile("public class Sample { }", OutputKind.DynamicallyLinkedLibrary, "Library");
        using var first = Read(image);
        using var second = Read(image);
        Assert.Throws<ArgumentException>(() => new Xui.Compiler.XuiNet.Compilation(first, second));
        Assert.Null(first.ManifestModule.Compilation);
        Assert.Null(second.ManifestModule.Compilation);
    }

    private static Model.Assembly Read(byte[] image)
    {
        using var stream = new MemoryStream(image);
        return AssemblyCompiler.Read(stream);
    }
}
