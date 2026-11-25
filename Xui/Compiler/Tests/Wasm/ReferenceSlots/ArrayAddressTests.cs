using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.ReferenceSlots;

[Collection(nameof(ManagedConsoleCollection))]
public class ArrayAddressTests
{
    [Fact]
    public Task ElementAliasesAndFieldAliasesKeepDifferentOwners() => InlineArrayTests.AssertLinked("""
        public class Box { public int Value; }
        public static class Program {
            static ref int At(int[] values, int index) => ref values[index];
            static void Add(ref int first, ref int second) { first=20; second+=22; }
            public static int Main() {
                var values=new int[2]; var box=new Box();
                ref int alias=ref At(values, 1); Add(ref alias, ref values[1]);
                Add(ref box.Value, ref values[0]);
                return alias==42 && values[0]==22 && box.Value==20 ? 42 : 1;
            }
        }
        """, "", 42);

    [Fact]
    public Task ScalarAndReferenceElementsSupportGenericRefStores() => InlineArrayTests.AssertLinked("""
        public enum Tiny : byte { Max=255 }
        public class Box { public int Value; }
        public static class Program {
            static void Set<T>(ref T place, T value) { place=value; }
            public static int Main() {
                var b=new byte[1]; var s=new short[1]; var l=new long[1];
                var f=new float[1]; var d=new double[1]; var c=new char[1];
                var flag=new bool[1]; var e=new Tiny[1]; var text=new string[1]; var objects=new Box[1];
                Set(ref b[0], (byte)255); Set(ref s[0], (short)-123); Set(ref l[0], long.MinValue);
                Set(ref f[0], 1.25f); Set(ref d[0], 2.5); Set(ref c[0], 'λ');
                Set(ref flag[0], true); Set(ref e[0], Tiny.Max); Set(ref text[0], "hello");
                var box=new Box(); box.Value=42; Set(ref objects[0], box);
                return b[0]==255 && s[0]==-123 && l[0]==long.MinValue && f[0]==1.25f && d[0]==2.5 &&
                    c[0]=='λ' && flag[0] && e[0]==Tiny.Max && text[0]=="hello" && objects[0]==box ? objects[0].Value : 1;
            }
        }
        """, "", 42);

    [Fact]
    public Task AddressCreationChecksNullAndBounds() => InlineArrayTests.AssertLinked("""
        using System;
        public static class Program {
            static void Set(ref int value) { value=1; }
            public static int Main() {
                int result=0; int[] absent=null; var values=new int[1];
                try { Set(ref absent[0]); } catch(NullReferenceException) { result+=14; }
                try { Set(ref values[-1]); } catch(IndexOutOfRangeException) { result+=14; }
                try { Set(ref values[1]); } catch(IndexOutOfRangeException) { result+=14; }
                return result;
            }
        }
        """, "", 42);
}
