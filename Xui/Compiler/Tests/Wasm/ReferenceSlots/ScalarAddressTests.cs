using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.ReferenceSlots;

[Collection(nameof(ManagedConsoleCollection))]
public class ScalarAddressTests
{
    [Fact]
    public async Task AddressTakenScalarCanBeInitializedIndirectlyWithoutInitLocals()
    {
        byte[] image = Frontend.Msil.Compile(il =>
        {
            il.LoadLocalAddress(0); il.LoadConstantI4(42);
            il.OpCode(System.Reflection.Metadata.ILOpCode.Stind_i4);
            il.LoadLocal(0); il.OpCode(System.Reflection.Metadata.ILOpCode.Ret);
        }, [System.Reflection.Metadata.SignatureTypeCode.Int32], initLocals: false);
        using var assembly = Canvas.CanvasTests.Read(image);
        byte[] wasm = new Xui.Compiler.WASM.Emitter.WasmEmitter().Emit(assembly);
        Assert.Equal(42, (await Repl.Node.ExecuteAsync(wasm)).ExitCode);
    }

    [Fact]
    public Task ScalarLocalsAndArgumentsHaveStableAliasingAddresses() => InlineArrayTests.AssertLinked("""
        using System;
        public static class Program {
            static void Change(ref int a,ref int b) { a=10; b+=32; }
            static int Argument(int value) { ref int alias=ref value; value=7; Console.WriteLine(alias); Change(ref value,ref value); return value; }
            public static int Main() {
                int value=0; ref int alias=ref value;
                Change(ref value,ref alias); Console.WriteLine(value); alias=9; Console.WriteLine(value);
                Console.WriteLine(Argument(1)); return 42;
            }
        }
        """, "42\n9\n7\n42\n", 42);

    [Fact]
    public Task NarrowWideFloatingAndEnumAddressesRetainTheirStorageTypes() => InlineArrayTests.AssertLinked("""
        using System;
        using System.Runtime.InteropServices;
        public enum ByteEnum : byte { A=1,B=255 }
        public static class Program {
            static void Change(ref byte b,ref short s,ref long l,ref float f,ref double d,ref bool flag,ref char c,ref ByteEnum e,ref NFloat n) {
                b=255; s=-123; l=long.MinValue; f=1.25f; d=2.5; flag=true; c='Ж'; e=ByteEnum.B; n=(NFloat)3.5;
            }
            public static int Main() {
                byte b=0; short s=0; long l=0; float f=0; double d=0; bool flag=false; char c='a'; ByteEnum e=ByteEnum.A; NFloat n=0;
                Change(ref b,ref s,ref l,ref f,ref d,ref flag,ref c,ref e,ref n);
                Console.WriteLine(b); Console.WriteLine(s); Console.WriteLine(l); Console.WriteLine(f); Console.WriteLine(d);
                Console.WriteLine(flag); Console.WriteLine((int)c); Console.WriteLine((int)e); Console.WriteLine((double)n);
                return 42;
            }
        }
        """, "255\n-123\n-9223372036854775808\n1.25\n2.5\nTrue\n1046\n255\n3.5\n", 42);

    [Fact]
    public Task GenericScalarCopiesAndOutInitializationUseTheSameAddresses() => InlineArrayTests.AssertLinked("""
        using System;
        public static class Program {
            static T Read<T>(in T value) => value;
            static void Set<T>(ref T value,T next) { value=next; }
            static void Clear<T>(out T value) { value=default; }
            public static int Main() {
                byte b=255; Console.WriteLine(Read(in b)); Set(ref b,(byte)7); Console.WriteLine(b);
                Clear(out b); Console.WriteLine(b);
                double d=3.5; Console.WriteLine(Read(in d)); Clear(out d); Console.WriteLine(d);
                bool flag=true; Clear(out flag); Console.WriteLine(flag); return 42;
            }
        }
        """, "255\n7\n0\n3.5\n0\nFalse\n", 42);

    [Fact]
    public Task ScalarFramesUnwindWithoutLosingTheCallersValue() => InlineArrayTests.AssertLinked("""
        using System;
        public static class Program {
            static void Throw(ref int value) { int local=9; Add(ref local); value=local; throw new InvalidOperationException(); }
            static void Add(ref int value) { value+=33; }
            public static int Main() {
                int value=0;
                for(int i=0;i<100000;i++) { try { Throw(ref value); } catch(InvalidOperationException) { } }
                Console.WriteLine(value); return 42;
            }
        }
        """, "42\n", 42);
}
