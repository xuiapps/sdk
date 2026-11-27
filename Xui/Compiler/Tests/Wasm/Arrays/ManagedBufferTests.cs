using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Arrays;

[Collection(nameof(ManagedConsoleCollection))]
public class ManagedBufferTests
{
    [Fact]
    public Task ArrayReferencesKeepTheirOwnerAcrossTypedOffsetArithmetic() => InlineArrayTests.AssertLinked("""
        using System;
        using System.Runtime.CompilerServices;
        using System.Runtime.InteropServices;
        struct Entry { public int Number; public string Text; }
        public static class Program {
            static ref T Next<T>(ref T value,int offset)=>ref Unsafe.Add(ref value,offset);
            public static int Main() {
                var values=new Entry[3];
                ref var first=ref MemoryMarshal.GetArrayDataReference(values);
                Next(ref first,2)=new Entry {Number=42,Text="last"};
                ref var middle=ref values[1];
                Console.WriteLine(Next(ref middle,1).Text);
                Unsafe.Add(ref middle,(nint)(-1)).Text="first"; Console.WriteLine(first.Text);
                Next(ref middle,-1).Number=7; Console.WriteLine(first.Number==7);
                var numbers=new int[] {2,4,6};
                ref int number=ref MemoryMarshal.GetArrayDataReference(numbers); Unsafe.Add(ref number,(nuint)2)=9;
                Console.WriteLine(numbers[2]);
                var texts=new string[] {"a","b"}; Next(ref texts[0],1)="changed"; Console.WriteLine(texts[1]);
                Console.WriteLine(Unsafe.IsNullRef(ref MemoryMarshal.GetArrayDataReference(Array.Empty<int>())));
                try { ref var missing=ref MemoryMarshal.GetArrayDataReference<int>(null); } catch(NullReferenceException) { Console.WriteLine("null"); }
                return Next(ref middle,1).Number;
            }
        }
        ""","last\nfirst\nTrue\n9\nchanged\nFalse\nnull\n",42);
}
