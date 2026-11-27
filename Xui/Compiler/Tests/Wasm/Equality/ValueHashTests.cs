using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Equality;

[Collection(nameof(ManagedConsoleCollection))]
public class ValueHashTests
{
    [Fact]
    public Task AcquiringComparerDoesNotRequireUnusedEqualityOrHashImplementations() => InlineArrayTests.AssertLinked("""
        using System.Collections.Generic;
        public struct Payload { public int Value; public string Text; }
        public static class Program {
            public static int Main() {
                var first = EqualityComparer<Payload>.Default;
                return first != null && object.ReferenceEquals(first, EqualityComparer<Payload>.Default) ? 42 : 0;
            }
        }
        """, "", 42);

    [Fact]
    public void CallingUnsupportedStructEqualityStillFailsExplicitly()
    {
        using var app = Xui.Compiler.Tests.Wasm.Canvas.CanvasTests.Read(Frontend.Roslyn.Compile("""
            using System.Collections.Generic;
            public struct Payload { public int Value; public bool Equals(Payload other) => true; }
            public static class Program {
                public static int Main() => EqualityComparer<Payload>.Default.Equals(new Payload(), new Payload()) ? 1 : 0;
            }
            """));
        using var framework = new Frontend.Framework();
        _ = new Xui.Compiler.XuiNet.Compilation([app, .. framework.Assemblies]);
        var error = Assert.Throws<NotSupportedException>(() => new Xui.Compiler.WASM.Emitter.WasmEmitter().Emit(app));
        Assert.Contains("Default equality is not implemented for Payload", error.Message);
    }

    [Fact]
    public Task OverridesHashCopiesOfLinearAndManagedStructValues() => InlineArrayTests.AssertLinked("""
        using System;
        using System.Collections.Generic;
        public struct Linear {
            public int Value;
            public override int GetHashCode()=>++Value;
        }
        public struct Managed {
            public string Text; public int Value;
            public override int GetHashCode() { Text=null; return ++Value; }
        }
        public static class Program {
            public static int Main() {
                var a=new Linear {Value=20}; var b=new Managed {Text="kept",Value=22};
                Console.WriteLine(EqualityComparer<Linear>.Default.GetHashCode(a)); Console.WriteLine(a.Value);
                IEqualityComparer<Managed> comparer=EqualityComparer<Managed>.Default;
                Console.WriteLine(comparer.GetHashCode(b)); Console.WriteLine(b.Text); Console.WriteLine(b.Value);
                return a.Value+b.Value;
            }
        }
        """, "21\n20\n23\nkept\n22\n", 42, System.Reflection.Assembly.Load("System.Collections"));

    [Fact]
    public Task TuplesAndRecordStructsUseTheirManagedHashImplementations() => InlineArrayTests.AssertLinked("""
        using System;
        using System.Collections.Generic;
        public readonly record struct Style(string Color, int Width);
        public static class Program {
            public static int Main() {
                var comparer=EqualityComparer<(int,Style,double)>.Default;
                var a=(1,new Style("blue",2),1.5); var b=(1,new Style("blue",2),1.5);
                Console.WriteLine(comparer.Equals(a,(2,new Style("blue",2),1.5)));
                return comparer.Equals(a,b) && comparer.GetHashCode(a)==comparer.GetHashCode(b) ? 42 : 1;
            }
        }
        """, "False\n", 42, System.Reflection.Assembly.Load("System.Collections"));
}
