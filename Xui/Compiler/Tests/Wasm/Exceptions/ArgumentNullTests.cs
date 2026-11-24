using Xui.Compiler.Tests.Wasm.Gc;
using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Exceptions;

[Collection(nameof(ManagedConsoleCollection))]
public class ArgumentNullTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public Task ConstructorsPreserveParameterNamesAndMessages(bool linked)
    {
        const string source = """
            using System;
            public static class Program {
                static void Print(ArgumentException error) {
                    Console.WriteLine(error.ParamName); Console.WriteLine(error.Message);
                }
                public static int Main() {
                    Print(new ArgumentNullException());
                    Print(new ArgumentNullException("child"));
                    Print(new ArgumentNullException((string)null));
                    Print(new ArgumentNullException(""));
                    Print(new ArgumentNullException("child", "custom"));
                    Print(new ArgumentNullException("child", (string)null));
                    Print(new ArgumentNullException("child", ""));
                    Print(new ArgumentNullException(null, "custom"));
                    Print(new ArgumentException("custom", "value"));
                    Print(new ArgumentException("plain"));
                    return 42;
                }
            }
            """;
        const string output = "\nValue cannot be null.\nchild\nValue cannot be null. (Parameter 'child')\n\nValue cannot be null.\n\nValue cannot be null.\nchild\ncustom (Parameter 'child')\nchild\nValue cannot be null. (Parameter 'child')\nchild\n (Parameter 'child')\n\ncustom\nvalue\ncustom (Parameter 'value')\n\nplain\n";
        return linked ? InlineArrayTests.AssertLinked(source, output, 42) : GcTests.AssertExecution(source, output, 42);
    }

    [Fact]
    public Task CatchHierarchyTypeTestsAndRethrowPreserveIdentity() => GcTests.AssertExecution("""
        using System;
        public static class Program {
            static void Fail(Exception error) {
                try { throw error; }
                catch (ArgumentException caught) {
                    Console.WriteLine(caught.ParamName); Console.WriteLine(caught == error); throw;
                }
            }
            public static int Main() {
                Exception error = new ArgumentNullException("child");
                Console.WriteLine(error is ArgumentException);
                Console.WriteLine(error is SystemException);
                Console.WriteLine(error is ArgumentNullException);
                Console.WriteLine(new ArgumentException() is ArgumentNullException);
                try { Fail(error); }
                catch (InvalidOperationException) { Console.WriteLine("wrong catch"); }
                catch (ArgumentNullException caught) { Console.WriteLine(caught == error); }
                finally { Console.WriteLine("finally"); }
                try { throw error; } catch (SystemException) { Console.WriteLine("system"); }
                return 42;
            }
        }
        """, "True\nTrue\nTrue\nFalse\nchild\nTrue\nTrue\nfinally\nsystem\n", 42);

    [Fact]
    public Task DynamicParameterNamesAndNullAccessorsAreHandled() => GcTests.AssertExecution("""
        using System;
        public static class Program {
            static string Name(bool first) => first ? "child" : "other";
            static void Guard(object child, string name) { if (child is null) throw new ArgumentNullException(name); }
            public static int Main() {
                try { Guard(null, Name(false)); }
                catch (ArgumentException error) { Console.WriteLine(error.ParamName); Console.WriteLine(error.Message); }
                try { ArgumentException error = null; Console.WriteLine(error.ParamName); }
                catch (NullReferenceException) { Console.WriteLine("null getter"); }
                return 42;
            }
        }
        """, "other\nValue cannot be null. (Parameter 'other')\nnull getter\n", 42);
}
