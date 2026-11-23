using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;

namespace Xui.Compiler.Tests.Frontend;

/// <summary>Builds an exact-IL static int Main for stack and emission tests.</summary>
internal static class Msil
{
    public static byte[] Compile(Action<InstructionEncoder> emit, SignatureTypeCode[]? locals = null, bool initLocals = true)
    {
        var metadata = new MetadataBuilder();
        metadata.AddModule(0, metadata.GetOrAddString("Graph.exe"), metadata.GetOrAddGuid(Guid.NewGuid()), default, default);
        metadata.AddAssembly(metadata.GetOrAddString("Graph"), new Version(1, 0), default, default, 0, AssemblyHashAlgorithm.None);
        var core = typeof(object).Assembly.GetName();
        var coreRef = metadata.AddAssemblyReference(metadata.GetOrAddString(core.Name!), core.Version!, default,
            metadata.GetOrAddBlob(core.GetPublicKeyToken()!), 0, default);
        var objectRef = metadata.AddTypeReference(coreRef, metadata.GetOrAddString("System"), metadata.GetOrAddString("Object"));
        metadata.AddTypeDefinition(TypeAttributes.NotPublic, default, metadata.GetOrAddString("<Module>"), default,
            MetadataTokens.FieldDefinitionHandle(1), MetadataTokens.MethodDefinitionHandle(1));
        metadata.AddTypeDefinition(TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.Sealed, default,
            metadata.GetOrAddString("Program"), objectRef, MetadataTokens.FieldDefinitionHandle(1), MetadataTokens.MethodDefinitionHandle(1));
        // Keep short-branch fixups within one blob in the SDK's method-body encoder.
        var code = new BlobBuilder(4096);
        var il = new InstructionEncoder(code, new ControlFlowBuilder());
        emit(il);
        StandaloneSignatureHandle localSignature = default;
        if (locals is { Length: > 0 })
        {
            var signatureBytes = new BlobBuilder();
            signatureBytes.WriteByte((byte)SignatureKind.LocalVariables);
            signatureBytes.WriteCompressedInteger(locals.Length);
            foreach (var type in locals) signatureBytes.WriteByte((byte)type);
            localSignature = metadata.AddStandaloneSignature(metadata.GetOrAddBlob(signatureBytes));
        }
        var bodies = new BlobBuilder();
        int body = new MethodBodyStreamEncoder(bodies).AddMethodBody(il, 8, localSignature,
            initLocals ? MethodBodyAttributes.InitLocals : MethodBodyAttributes.None);
        var signature = new BlobBuilder();
        new BlobEncoder(signature).MethodSignature().Parameters(0, result => result.Type().Int32(), _ => { });
        var main = metadata.AddMethodDefinition(MethodAttributes.Public | MethodAttributes.Static, MethodImplAttributes.IL,
            metadata.GetOrAddString("Main"), metadata.GetOrAddBlob(signature), body, MetadataTokens.ParameterHandle(1));
        var pe = new ManagedPEBuilder(new PEHeaderBuilder(imageCharacteristics: Characteristics.ExecutableImage),
            new MetadataRootBuilder(metadata), bodies, entryPoint: main);
        var image = new BlobBuilder();
        pe.Serialize(image);
        return image.ToArray();
    }
}
