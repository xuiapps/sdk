using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Exceptions;

[Collection(nameof(ManagedConsoleCollection))]
public class NestedScopeTests
{
    [Fact]
    public Task ConditionalScopesAndForeachDisposeAndContinue() => InlineArrayTests.AssertLinked("""
        using System;
        using System.Collections.Generic;
        ref struct Scope {
            public bool NeedsRedraw;
            public Scope(bool redraw) { NeedsRedraw=redraw; }
            public void Dispose() { Console.WriteLine("dispose"); }
        }
        public static class Program {
            static int sum;
            static void Run(bool redraw) {
                var cards=new List<int>(); cards.Add(20); cards.Add(21);
                sum=0;
                using var viewport=new Scope(redraw);
                if(viewport.NeedsRedraw) {
                    foreach(var card in cards) sum+=card;
                    using var indicator=new Scope(redraw);
                    if(indicator.NeedsRedraw) sum++;
                }

            }
            public static int Main() { Run(false); Console.WriteLine(sum); Run(true); return sum; }
        }
        """, "dispose\n0\ndispose\ndispose\n", 42, System.Reflection.Assembly.Load("System.Collections"));
}
