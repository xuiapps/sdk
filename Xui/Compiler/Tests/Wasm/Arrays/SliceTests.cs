using Xui.Compiler.Tests.Wasm.Canvas;
using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xui.Compiler.XuiNet.CodeFlow;
using Xui.Compiler.XuiNet.DotNet;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Arrays;

[Collection(nameof(ManagedConsoleCollection))]
public class SliceTests
{
    [Fact]
    public Task CoreLibSlicesKeepReferenceIdentityAndStructCopies() => InlineArrayTests.AssertLinked("""
        using System;
        sealed class Item { public int Number; }
        struct Value { public int Number; public string Text; }
        public static class Program {
            static T[] Slice<T>(T[] values,Range range)=>values[range];
            public static int Main() {
                var item=new Item {Number=42}; var items=new Item[] {new Item(),item,new Item()};
                var slice=Slice(items,1..^1); Console.WriteLine(slice.Length); Console.WriteLine(ReferenceEquals(item,slice[0]));
                slice[0].Number=20; Console.WriteLine(item.Number);
                var values=new Value[] {new Value {Number=22,Text="original"}};
                var copies=Slice(values,..); copies[0].Text="changed"; Console.WriteLine(values[0].Text);
                Console.WriteLine(Slice(items,^0..).Length);
                var texts=Slice(new string[] {"first","second"},1..); Console.WriteLine(texts[0]);
                try { Slice<Item>(null,..); } catch(ArgumentNullException e) { Console.WriteLine(e.ParamName); }
                try { Slice(items,2..1); } catch(ArgumentOutOfRangeException) { Console.WriteLine("range"); }
                try { Slice(items,..4); } catch(ArgumentOutOfRangeException) { Console.WriteLine("bounds"); }
                return slice[0].Number+copies[0].Number;
            }
        }
        ""","1\nTrue\n20\noriginal\n0\nsecond\narray\nrange\nbounds\n",42);

    [Fact]
    public Task ExactArrayTypeFoldingPreservesNullFailures() => InlineArrayTests.AssertLinked("""
        using System;
        sealed class Item {}
        public static class Program {
            static bool Equal(Item[] values)=>typeof(Item[])==values.GetType();
            static bool Different(Item[] values)=>typeof(Item[])!=values.GetType();
            public static int Main() {
                Console.WriteLine(Equal(new Item[0])); Console.WriteLine(Different(new Item[0]));
                try { Equal(null); } catch(NullReferenceException) { Console.WriteLine("null"); }
                return 42;
            }
        }
        ""","True\nFalse\nnull\n",42);

    [Theory]
    [InlineData("int")]
    [InlineData("Kind")]
    [InlineData("Base")]
    public void CovariantAndReducedIntegerArrayTypesAreNotAssumedExact(string type)
    {
        using var app=CanvasTests.Read(Frontend.Roslyn.Compile($$"""
            enum Kind { Value } class Base {} class Derived : Base {}
            public static class Program {
                static bool Check({{type}}[] values)=>typeof({{type}}[])==values.GetType();
                public static int Main()=>0;
            }
            """));
        using var framework=new Frontend.Framework();
        _=new Xui.Compiler.XuiNet.Compilation([app,..framework.Assemblies]);
        var module=app.ManifestModule;
        var handle=module.Methods.Single(h=>module.GetMethod(h).Name=="Check");
        var body=module.GetMethod(handle).Body!;
        var graph=InstructionGraph.Read(body.Instructions);
        TypeSpecialization.Apply(module,body,MethodIdentity.FromDefinition(module,handle),graph);
        Assert.DoesNotContain(graph.Specializations.Values,s=>s.Kind==SpecializationKind.NullCheckAndInt32);
    }
}
