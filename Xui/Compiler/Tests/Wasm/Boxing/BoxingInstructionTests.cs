using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.Tests.Wasm.Canvas;
using Xui.Compiler.Tests.Wasm.Gc;
using Xui.Compiler.Tests.Wasm.Repl;
using Xui.Compiler.Tests.Wasm.Structs;
using Xui.Compiler.WASM.Emitter;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Boxing;

[Collection(nameof(ManagedConsoleCollection))]
public class BoxingInstructionTests
{
    [Theory]
    [InlineData(SignatureTypeCode.Int32)]
    [InlineData(SignatureTypeCode.UInt32)]
    [InlineData(SignatureTypeCode.Byte)]
    [InlineData(SignatureTypeCode.SByte)]
    [InlineData(SignatureTypeCode.Int16)]
    [InlineData(SignatureTypeCode.UInt16)]
    [InlineData(SignatureTypeCode.Char)]
    [InlineData(SignatureTypeCode.Boolean)]
    [InlineData(SignatureTypeCode.Int64)]
    [InlineData(SignatureTypeCode.UInt64)]
    [InlineData(SignatureTypeCode.Single)]
    [InlineData(SignatureTypeCode.Double)]
    [InlineData(SignatureTypeCode.IntPtr)]
    [InlineData(SignatureTypeCode.UIntPtr)]
    public Task BoxPayloadSupportsTypedLoadsStoresAndInitialization(SignatureTypeCode code)
    {
        TypeReferenceHandle type = default;
        void Constant(InstructionEncoder il, int value)
        {
            if (code is SignatureTypeCode.Int64 or SignatureTypeCode.UInt64) il.LoadConstantI8(value);
            else if (code == SignatureTypeCode.Single) il.LoadConstantR4(value);
            else if (code == SignatureTypeCode.Double) il.LoadConstantR8(value);
            else { il.LoadConstantI4(value); if (code is SignatureTypeCode.IntPtr or SignatureTypeCode.UIntPtr) il.OpCode(ILOpCode.Conv_i); }
        }
        void Compare(InstructionEncoder il, int value)
        {
            if (code is SignatureTypeCode.IntPtr or SignatureTypeCode.UIntPtr) { il.OpCode(ILOpCode.Conv_i4); il.LoadConstantI4(value); }
            else Constant(il, value);
            il.OpCode(ILOpCode.Ceq);
        }
        var image = Frontend.Msil.Compile(il =>
        {
            Constant(il, 0); il.OpCode(ILOpCode.Box); il.Token(type); il.StoreLocal(0);
            il.LoadLocal(0); il.OpCode(ILOpCode.Unbox); il.Token(type);
            Constant(il, 1); il.OpCode(ILOpCode.Stobj); il.Token(type);
            il.LoadLocal(0); il.OpCode(ILOpCode.Unbox); il.Token(type);
            il.OpCode(ILOpCode.Ldobj); il.Token(type); Compare(il, 1); il.StoreLocal(1);
            il.LoadLocal(0); il.OpCode(ILOpCode.Unbox); il.Token(type); il.OpCode(ILOpCode.Initobj); il.Token(type);
            il.LoadLocal(0); il.OpCode(ILOpCode.Unbox_any); il.Token(type); Compare(il, 0);
            il.LoadLocal(1); il.OpCode(ILOpCode.And); il.LoadConstantI4(42); il.OpCode(ILOpCode.Mul); il.OpCode(ILOpCode.Ret);
        }, [SignatureTypeCode.Object, SignatureTypeCode.Int32], configureMetadata: metadata =>
            type=metadata.AddTypeReference(MetadataTokens.AssemblyReferenceHandle(1), metadata.GetOrAddString("System"), metadata.GetOrAddString(code.ToString())));
        return AssertLinked(image);
    }

    [Theory]
    [InlineData(ILOpCode.Unbox, true)]
    [InlineData(ILOpCode.Unbox, false)]
    [InlineData(ILOpCode.Castclass, false)]
    public Task NullAndWrongBoxChecksThrowManagedExceptions(ILOpCode operation, bool isNull)
    {
        TypeReferenceHandle integer=default, wrong=default, exception=default;
        var image=Frontend.Msil.Compile(il =>
        {
            var start=il.DefineLabel(); var caught=il.DefineLabel(); var end=il.DefineLabel();
            il.MarkLabel(start);
            if(isNull) il.OpCode(ILOpCode.Ldnull);
            else { il.LoadConstantI4(1); il.OpCode(ILOpCode.Box); il.Token(wrong); }
            il.OpCode(operation); il.Token(integer); il.OpCode(ILOpCode.Pop);
            il.Branch(ILOpCode.Leave, end);
            il.MarkLabel(caught); il.OpCode(ILOpCode.Pop); il.LoadConstantI4(42); il.StoreLocal(0);
            il.Branch(ILOpCode.Leave, end);
            il.MarkLabel(end); il.LoadLocal(0); il.OpCode(ILOpCode.Ret);
            il.ControlFlowBuilder!.AddCatchRegion(start, caught, caught, end, exception);
        }, [SignatureTypeCode.Int32], configureMetadata: metadata =>
        {
            TypeReferenceHandle Type(string name)=>metadata.AddTypeReference(MetadataTokens.AssemblyReferenceHandle(1),metadata.GetOrAddString("System"),metadata.GetOrAddString(name));
            integer=Type("Int32"); wrong=Type("UInt32"); exception=Type(isNull ? "NullReferenceException" : "InvalidCastException");
        });
        return AssertLinked(image);
    }

    [Fact]
    public Task CastclassToBoxedTypePreservesNull()
    {
        TypeReferenceHandle integer=default;
        var image=Frontend.Msil.Compile(il=>
        {
            il.OpCode(ILOpCode.Ldnull); il.OpCode(ILOpCode.Castclass); il.Token(integer);
            il.OpCode(ILOpCode.Ldnull); il.OpCode(ILOpCode.Ceq); il.LoadConstantI4(42); il.OpCode(ILOpCode.Mul); il.OpCode(ILOpCode.Ret);
        },configureMetadata: metadata=>integer=metadata.AddTypeReference(MetadataTokens.AssemblyReferenceHandle(1),metadata.GetOrAddString("System"),metadata.GetOrAddString("Int32")));
        return AssertLinked(image);
    }

    private static async Task AssertLinked(byte[] image)
    {
        var managed=GcTests.RunManaged(image);
        Assert.Equal(42, managed.ExitCode);
        using var app=CanvasTests.Read(image);
        using var framework=new Frontend.Framework();
        _=new Xui.Compiler.XuiNet.Compilation([app, .. framework.Assemblies]);
        Assert.Equal(managed, await Node.ExecuteAsync(new WasmEmitter().Emit(app)));
    }
}
