using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Strings;

[Collection(nameof(ManagedConsoleCollection))]
public class ObjectStorageTests
{
    [Fact]
    public Task StraightLineStringArgumentsAndReturnsCreateObjectCarriers() => InlineArrayTests.AssertLinked("""
        public static class Program {
            static object Wrap(string value)=>value;
            static string Unwrap(object value)=>(string)value;
            public static int Main()=>Unwrap(Wrap("hello")).Length+37;
        }
        """, "", 42);

    [Fact]
    public Task StringsRoundTripThroughObjectSlotsAndGenericCasts() => InlineArrayTests.AssertLinked("""
        using System;
        class Holder { public object Value; }
        public static class Program {
            static object Store<T>(T value) => value;
            static T Read<T>(object value) => (T)value;
            public static int Main() {
                var holder=new Holder {Value="hello"};
                object[] array={Store("😀"),holder.Value,null};
                if(Read<string>(array[0])!="😀" || (string)array[1]!="hello" || (string)array[2]!=null) return 1;
                if(!(holder.Value is string) || new Holder() is string) return 2;
                object first="hello", second="hello";
                if(first!=second || holder.Value!=first) return 3;
                if(first.GetType()!=typeof(string) || string.Format("[{0}]",first)!="[hello]") return 5;
                bool rejected=false;
                try { Console.WriteLine((string)(object)new Holder()); }
                catch(InvalidCastException) { rejected=true; }
                return rejected && (new Holder() as object as string)==null ? 42 : 4;
            }
        }
        """, "", 42);
}
