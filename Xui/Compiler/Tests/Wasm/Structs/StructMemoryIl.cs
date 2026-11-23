using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;

namespace Xui.Compiler.Tests.Wasm.Structs;

// Exact IL exercises value snapshots and cpobj/ldobj/stobj that Roslyn may optimize away.
internal static class StructMemoryIl
{
    public static byte[] Compile(bool withReferenceField = false)
    {
        var metadata = new MetadataBuilder();
        metadata.AddModule(0, metadata.GetOrAddString("StructMemory.exe"), metadata.GetOrAddGuid(Guid.NewGuid()), default, default);
        metadata.AddAssembly(metadata.GetOrAddString("StructMemory"), new Version(1, 0), default, default, 0, AssemblyHashAlgorithm.None);
        var core = typeof(object).Assembly.GetName();
        var coreRef = metadata.AddAssemblyReference(metadata.GetOrAddString(core.Name!), core.Version!, default,
            metadata.GetOrAddBlob(core.GetPublicKeyToken()!), 0, default);
        var objectRef = metadata.AddTypeReference(coreRef, metadata.GetOrAddString("System"), metadata.GetOrAddString("Object"));
        var valueRef = metadata.AddTypeReference(coreRef, metadata.GetOrAddString("System"), metadata.GetOrAddString("ValueType"));
        metadata.AddTypeDefinition(TypeAttributes.NotPublic, default, metadata.GetOrAddString("<Module>"), default,
            MetadataTokens.FieldDefinitionHandle(1), MetadataTokens.MethodDefinitionHandle(1));
        var data = metadata.AddTypeDefinition(TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.SequentialLayout,
            default, metadata.GetOrAddString("Data"), valueRef, MetadataTokens.FieldDefinitionHandle(1), MetadataTokens.MethodDefinitionHandle(1));
        metadata.AddTypeDefinition(TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.Sealed, default,
            metadata.GetOrAddString("Program"), objectRef, MetadataTokens.FieldDefinitionHandle(withReferenceField ? 3 : 2), MetadataTokens.MethodDefinitionHandle(1));
        var field = metadata.AddFieldDefinition(FieldAttributes.Public, metadata.GetOrAddString("Value"),
            metadata.GetOrAddBlob(new byte[] { (byte)SignatureKind.Field, (byte)SignatureTypeCode.Int32 }));
        if (withReferenceField)
            metadata.AddFieldDefinition(FieldAttributes.Public, metadata.GetOrAddString("Text"),
                metadata.GetOrAddBlob(new byte[] { (byte)SignatureKind.Field, (byte)SignatureTypeCode.String }));
        var localBytes = new BlobBuilder();
        var locals = new BlobEncoder(localBytes).LocalVariableSignature(2);
        locals.AddVariable().Type().Type(data, isValueType: true);
        locals.AddVariable().Type().Type(data, isValueType: true);
        var localSignature = metadata.AddStandaloneSignature(metadata.GetOrAddBlob(localBytes));
        var il = new InstructionEncoder(new BlobBuilder());
        Store(0, 7);
        il.LoadLocal(0); // Capture 7 as a value, then overwrite its source.
        Store(0, 90);
        Token(ILOpCode.Ldfld, field); // Must still read 7.
        il.LoadLocalAddress(1); il.LoadLocalAddress(0); Token(ILOpCode.Cpobj, data);
        Store(0, 100);
        il.LoadLocalAddress(1); Token(ILOpCode.Ldobj, data); // Capture 90.
        Store(1, 200);
        Token(ILOpCode.Ldfld, field); il.OpCode(ILOpCode.Add); // 7 + 90.
        il.LoadLocalAddress(0); il.LoadLocal(1); Token(ILOpCode.Stobj, data);
        il.LoadLocalAddress(0); il.OpCode(ILOpCode.Dup); Token(ILOpCode.Cpobj, data); // Self-copy.
        il.LoadLocalAddress(0); Token(ILOpCode.Ldfld, field); il.OpCode(ILOpCode.Add); // + 200.
        il.OpCode(ILOpCode.Ret);
        var bodies = new BlobBuilder();
        int body = new MethodBodyStreamEncoder(bodies).AddMethodBody(il, 8, localSignature, MethodBodyAttributes.InitLocals);
        var signature = new BlobBuilder();
        new BlobEncoder(signature).MethodSignature().Parameters(0, result => result.Type().Int32(), _ => { });
        var main = metadata.AddMethodDefinition(MethodAttributes.Public | MethodAttributes.Static, MethodImplAttributes.IL,
            metadata.GetOrAddString("Main"), metadata.GetOrAddBlob(signature), body, MetadataTokens.ParameterHandle(1));
        var pe = new ManagedPEBuilder(new PEHeaderBuilder(imageCharacteristics: Characteristics.ExecutableImage),
            new MetadataRootBuilder(metadata), bodies, entryPoint: main);
        var image = new BlobBuilder();
        pe.Serialize(image);
        return image.ToArray();

        void Token(ILOpCode code, EntityHandle token) { il.OpCode(code); il.Token(token); }
        void Store(int local, int value) { il.LoadLocalAddress(local); il.LoadConstantI4(value); Token(ILOpCode.Stfld, field); }
    }
}
