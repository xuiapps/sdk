using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using Xui.Compiler.WASM.Emitter;
using Xui.Compiler.XuiNet;
using Xunit;

namespace Xui.Compiler.Tests.Wasm;

public class LocalTests
{
    [Theory]
    [InlineData(SignatureTypeCode.Boolean, 257, 1)]
    [InlineData(SignatureTypeCode.Boolean, 256, 0)]
    [InlineData(SignatureTypeCode.SByte, 255, -1)]
    [InlineData(SignatureTypeCode.Byte, -1, 255)]
    [InlineData(SignatureTypeCode.Int16, 65535, -1)]
    [InlineData(SignatureTypeCode.UInt16, -1, 65535)]
    [InlineData(SignatureTypeCode.Char, 65536 + 65, 65)]
    [InlineData(SignatureTypeCode.Int32, int.MinValue, int.MinValue)]
    [InlineData(SignatureTypeCode.UInt32, -1, -1)]
    public Task IntegerLocalsNarrowAndExtend(SignatureTypeCode type, int input, int expected) =>
        AssertExecutes(Program([type], il =>
        {
            il.LoadConstantI4(input); il.StoreLocal(0); il.LoadLocal(0);
        }), expected);

    [Theory]
    [InlineData(SignatureTypeCode.Int64)]
    [InlineData(SignatureTypeCode.UInt64)]
    public Task WideLocalsPreserveAllBits(SignatureTypeCode type) =>
        AssertExecutes(Program([type], il =>
        {
            il.LoadConstantI8(long.MinValue); il.StoreLocal(0); il.LoadLocal(0);
            il.LoadConstantI8(long.MinValue); il.OpCode(ILOpCode.Ceq);
        }), 1);

    [Theory]
    [InlineData(SignatureTypeCode.Single, 16777216.0)]
    [InlineData(SignatureTypeCode.Double, 16777217.0)]
    public Task FloatingLocalsUseTheirDeclaredPrecision(SignatureTypeCode type, double expected) =>
        AssertExecutes(Program([type], il =>
        {
            il.LoadConstantR8(16777217); il.StoreLocal(0); il.LoadLocal(0);
            il.LoadConstantR8(expected); il.OpCode(ILOpCode.Ceq);
        }), 1);

    [Theory]
    [InlineData(SignatureTypeCode.IntPtr, ILOpCode.Conv_i)]
    [InlineData(SignatureTypeCode.UIntPtr, ILOpCode.Conv_u)]
    public Task NativeLocalsRoundTrip(SignatureTypeCode type, ILOpCode conversion) =>
        AssertExecutes(Program([type], il =>
        {
            il.LoadConstantI4(-42); il.OpCode(conversion); il.StoreLocal(0); il.LoadLocal(0);
            il.OpCode(ILOpCode.Conv_i4);
        }), -42);

    [Fact]
    public Task NativeValuesCanBeStoredInSmallIntegerLocals() =>
        AssertExecutes(Program([SignatureTypeCode.Byte], il =>
        {
            il.LoadConstantI4(257); il.OpCode(ILOpCode.Conv_i); il.StoreLocal(0); il.LoadLocal(0);
        }), 1);

    [Fact]
    public Task Int32ValuesCanBeStoredInNativeLocals() =>
        AssertExecutes(Program([SignatureTypeCode.IntPtr], il =>
        {
            il.LoadConstantI4(-42); il.StoreLocal(0); il.LoadLocal(0); il.OpCode(ILOpCode.Conv_i4);
        }), -42);

    [Fact]
    public async Task NativeLocalsUseWasm32Width()
    {
        byte[] image = Program([SignatureTypeCode.IntPtr], il =>
        {
            il.LoadConstantI8(0x100000001); il.OpCode(ILOpCode.Conv_i); il.StoreLocal(0); il.LoadLocal(0);
            il.OpCode(ILOpCode.Conv_i8); il.LoadConstantI8(1); il.OpCode(ILOpCode.Ceq);
        });
        // The host CLR may be 64-bit; this assertion specifically checks our WASM32 target ABI.
        Assert.Equal(1, (await Repl.Node.ExecuteAsync(Emit(image))).ExitCode);
    }

    public static IEnumerable<object[]> LocalTypes => new[]
    {
        SignatureTypeCode.Boolean, SignatureTypeCode.Char, SignatureTypeCode.SByte, SignatureTypeCode.Byte,
        SignatureTypeCode.Int16, SignatureTypeCode.UInt16, SignatureTypeCode.Int32, SignatureTypeCode.UInt32,
        SignatureTypeCode.Int64, SignatureTypeCode.UInt64, SignatureTypeCode.Single, SignatureTypeCode.Double,
        SignatureTypeCode.IntPtr, SignatureTypeCode.UIntPtr, SignatureTypeCode.String
    }.Select(type => new object[] { type });

    [Theory]
    [MemberData(nameof(LocalTypes))]
    public Task InitLocalsInitializesEverySupportedType(SignatureTypeCode type) =>
        AssertExecutes(Program([type], il =>
        {
            il.LoadLocal(0);
            switch (type)
            {
                case SignatureTypeCode.Int64:
                case SignatureTypeCode.UInt64:
                    il.LoadConstantI8(0); il.OpCode(ILOpCode.Ceq); break;
                case SignatureTypeCode.Single:
                case SignatureTypeCode.Double:
                    il.LoadConstantR8(0); il.OpCode(ILOpCode.Ceq); break;
                case SignatureTypeCode.String:
                    il.OpCode(ILOpCode.Pop); il.LoadConstantI4(1); break;
                default:
                    if (type is SignatureTypeCode.IntPtr or SignatureTypeCode.UIntPtr) il.OpCode(ILOpCode.Conv_i4);
                    il.LoadConstantI4(0); il.OpCode(ILOpCode.Ceq); break;
            }
        }), 1);

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(255)]
    [InlineData(256)]
    public Task CompactShortAndWideLocalIndices(int index) =>
        AssertExecutes(Program(Enumerable.Repeat(SignatureTypeCode.Int32, index + 1).ToArray(), il =>
        {
            il.LoadConstantI4(42); il.StoreLocal(index);
            il.LoadConstantI4(7); il.StoreLocal(index);
            il.LoadLocal(index);
        }), 7);

    [Fact]
    public Task ExplicitWideEncodingWorksForSmallIndices() =>
        AssertExecutes(Program([SignatureTypeCode.Int32], il =>
        {
            il.LoadConstantI4(42);
            il.OpCode(ILOpCode.Stloc); il.CodeBuilder.WriteUInt16(0);
            il.OpCode(ILOpCode.Ldloc); il.CodeBuilder.WriteUInt16(0);
        }), 42);

    [Fact]
    public Task MixedLocalGroupsPreserveIndices() =>
        AssertExecutes(Program([SignatureTypeCode.Int32, SignatureTypeCode.Int32, SignatureTypeCode.Int64,
            SignatureTypeCode.Single, SignatureTypeCode.Double, SignatureTypeCode.Int32], il =>
        {
            il.LoadConstantI4(3); il.StoreLocal(0);
            il.LoadConstantI4(5); il.StoreLocal(1);
            il.LoadConstantI8(100); il.StoreLocal(2);
            il.LoadConstantR8(1.25); il.StoreLocal(3);
            il.LoadConstantR8(2.5); il.StoreLocal(4);
            il.LoadConstantI4(7); il.StoreLocal(5);
            il.LoadLocal(0); il.LoadLocal(1); il.OpCode(ILOpCode.Add);
            il.LoadLocal(5); il.OpCode(ILOpCode.Add);
        }), 15);

    [Fact]
    public void UninitializedReadIsRejected()
    {
        byte[] image = Program([SignatureTypeCode.Int32], il => il.LoadLocal(0), initLocals: false);
        Assert.Contains("Uninitialized local 0", Assert.Throws<InvalidProgramException>(() => Emit(image)).Message);
    }

    [Fact]
    public Task ExplicitStoreWithoutInitLocalsIsAllowed() =>
        AssertExecutes(Program([SignatureTypeCode.Int32], il =>
        {
            il.LoadConstantI4(42); il.StoreLocal(0); il.LoadLocal(0);
        }, initLocals: false), 42);

    [Fact]
    public void LocalInitializationIsTrackedPerSlot()
    {
        byte[] image = Program([SignatureTypeCode.Int32, SignatureTypeCode.Int32], il =>
        {
            il.LoadConstantI4(42); il.StoreLocal(0); il.LoadLocal(1);
        }, initLocals: false);
        Assert.Contains("Uninitialized local 1", Assert.Throws<InvalidProgramException>(() => Emit(image)).Message);
    }

    [Fact]
    public void InvalidLocalIndexIsRejected()
    {
        byte[] image = Program([SignatureTypeCode.Int32], il => il.LoadLocal(1));
        Assert.Contains("Invalid local index 1", Assert.Throws<InvalidProgramException>(() => Emit(image)).Message);
    }

    [Fact]
    public void StoreTypeMismatchIsRejected()
    {
        byte[] image = Program([SignatureTypeCode.Int32], il => { il.LoadConstantI8(1); il.StoreLocal(0); });
        Assert.Contains("type mismatch", Assert.Throws<InvalidProgramException>(() => Emit(image)).Message);
    }

    [Theory]
    [InlineData(SignatureTypeCode.Object)]
    [InlineData(SignatureTypeCode.TypedReference)]
    [InlineData(SignatureTypeCode.ByReference)]
    [InlineData(SignatureTypeCode.Pinned)]
    [InlineData(SignatureTypeCode.Pointer)]
    [InlineData(SignatureTypeCode.TypeHandle)]
    public void UnsupportedLocalSignaturesAreRejected(SignatureTypeCode type)
    {
        byte[] image = Program([type], il => il.LoadConstantI4(0));
        Assert.Contains("Unsupported local type", Assert.Throws<NotSupportedException>(() => Emit(image)).Message);
    }

    [Fact]
    public async Task InitializedStringLocalLogsAsAnEmptyLine()
    {
        byte[] image = Program([SignatureTypeCode.String], (il, metadata) =>
        {
            var identity = typeof(Console).Assembly.GetName();
            var reference = metadata.AddAssemblyReference(metadata.GetOrAddString(identity.Name!), identity.Version!, default,
                metadata.GetOrAddBlob(identity.GetPublicKeyToken()!), 0, default);
            var type = metadata.AddTypeReference(reference, metadata.GetOrAddString("System"), metadata.GetOrAddString("Console"));
            var signature = new BlobBuilder();
            new BlobEncoder(signature).MethodSignature().Parameters(1, result => result.Void(),
                parameters => parameters.AddParameter().Type().String());
            var writeLine = metadata.AddMemberReference(type, metadata.GetOrAddString("WriteLine"), metadata.GetOrAddBlob(signature));
            il.LoadLocal(0); il.Call(writeLine); il.LoadConstantI4(0);
        });
        using var stream = new MemoryStream(image);
        using var assembly = AssemblyCompiler.Read(stream);
        byte[] wasm = new WasmEmitter().Emit(assembly, new Rewrite.ConsoleWriteLineRewriter());
        var result = await Repl.Node.ExecuteAsync(wasm);
        Assert.Equal("\n", result.Output);
        Assert.Equal(0, result.ExitCode);
    }

    private static async Task AssertExecutes(byte[] image, int expected)
    {
        Assert.Equal(expected, (int)Assembly.Load(image).EntryPoint!.Invoke(null, null)!);
        var result = await Repl.Node.ExecuteAsync(Emit(image));
        Assert.Equal(expected, result.ExitCode);
        Assert.Empty(result.Output);
    }

    private static byte[] Emit(byte[] image)
    {
        using var stream = new MemoryStream(image);
        using var assembly = AssemblyCompiler.Read(stream);
        return new WasmEmitter().Emit(assembly);
    }

    private static byte[] Program(SignatureTypeCode[] locals, Action<InstructionEncoder> emit, bool initLocals = true) =>
        Program(locals, (il, _) => emit(il), initLocals);

    private static byte[] Program(SignatureTypeCode[] locals, Action<InstructionEncoder, MetadataBuilder> emit, bool initLocals = true)
    {
        // Build exact local signatures and IL, including forms Roslyn would optimize away.
        var metadata = new MetadataBuilder();
        metadata.AddModule(0, metadata.GetOrAddString("LocalTests.exe"), metadata.GetOrAddGuid(Guid.NewGuid()), default, default);
        metadata.AddAssembly(metadata.GetOrAddString("LocalTests"), new Version(1, 0), default, default, 0, AssemblyHashAlgorithm.None);
        var core = typeof(object).Assembly.GetName();
        var coreRef = metadata.AddAssemblyReference(metadata.GetOrAddString(core.Name!), core.Version!, default,
            metadata.GetOrAddBlob(core.GetPublicKeyToken()!), 0, default);
        var objectRef = metadata.AddTypeReference(coreRef, metadata.GetOrAddString("System"), metadata.GetOrAddString("Object"));
        metadata.AddTypeDefinition(TypeAttributes.NotPublic, default, metadata.GetOrAddString("<Module>"), default,
            MetadataTokens.FieldDefinitionHandle(1), MetadataTokens.MethodDefinitionHandle(1));
        metadata.AddTypeDefinition(TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.Sealed, default,
            metadata.GetOrAddString("Program"), objectRef, MetadataTokens.FieldDefinitionHandle(1), MetadataTokens.MethodDefinitionHandle(1));
        var localSignature = new BlobBuilder();
        localSignature.WriteByte((byte)SignatureKind.LocalVariables);
        localSignature.WriteCompressedInteger(locals.Length);
        foreach (var type in locals) localSignature.WriteByte((byte)type);
        var signatureHandle = metadata.AddStandaloneSignature(metadata.GetOrAddBlob(localSignature));
        var code = new BlobBuilder();
        var il = new InstructionEncoder(code);
        emit(il, metadata);
        il.OpCode(ILOpCode.Ret);
        var bodies = new BlobBuilder();
        int body = new MethodBodyStreamEncoder(bodies).AddMethodBody(il, 8, signatureHandle,
            initLocals ? MethodBodyAttributes.InitLocals : MethodBodyAttributes.None);
        var methodSignature = new BlobBuilder();
        new BlobEncoder(methodSignature).MethodSignature().Parameters(0, result => result.Type().Int32(), _ => { });
        var main = metadata.AddMethodDefinition(MethodAttributes.Public | MethodAttributes.Static, MethodImplAttributes.IL,
            metadata.GetOrAddString("Main"), metadata.GetOrAddBlob(methodSignature), body, MetadataTokens.ParameterHandle(1));
        var pe = new ManagedPEBuilder(new PEHeaderBuilder(imageCharacteristics: Characteristics.ExecutableImage),
            new MetadataRootBuilder(metadata), bodies, entryPoint: main);
        var image = new BlobBuilder();
        pe.Serialize(image);
        return image.ToArray();
    }
}
