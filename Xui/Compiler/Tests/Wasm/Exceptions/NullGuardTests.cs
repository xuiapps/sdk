using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Exceptions;

[Collection(nameof(ManagedConsoleCollection))]
public class NullGuardTests
{
    [Fact]
    public Task GuardPreservesArgumentNamesAndAcceptsAllReferenceRepresentations() => InlineArrayTests.AssertLinked("""
        using System;
        public class A { }
        public static class Program {
            public static int Main() {
                object value=null;
                try { ArgumentNullException.ThrowIfNull(value); }
                catch(ArgumentException e) { Console.WriteLine(e.ParamName); Console.WriteLine(e.Message); }
                try { ArgumentNullException.ThrowIfNull(value, ""); }
                catch(ArgumentNullException e) { Console.WriteLine(e.Message); }
                try { ArgumentNullException.ThrowIfNull(value, null); }
                catch(ArgumentNullException e) { Console.WriteLine(e.ParamName); Console.WriteLine(e.Message); }
                ArgumentNullException.ThrowIfNull("text"); ArgumentNullException.ThrowIfNull(new A());
                ArgumentNullException.ThrowIfNull(new int[2]);
                string text=null;
                try { ArgumentNullException.ThrowIfNull(text); } catch(ArgumentNullException e) { Console.WriteLine(e.ParamName); }
                return 42;
            }
        }
        """, "value\nValue cannot be null. (Parameter 'value')\nValue cannot be null.\n\nValue cannot be null.\ntext\n", 42);
}
