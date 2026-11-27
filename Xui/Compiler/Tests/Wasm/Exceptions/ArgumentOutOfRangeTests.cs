using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Exceptions;

[Collection(nameof(ManagedConsoleCollection))]
public class ArgumentOutOfRangeTests
{
    [Fact]
    public Task ActualValueIsRetainedAndFormattedOnlyWhenMessageIsRead() => InlineArrayTests.AssertLinked("""
        using System;
        class Payload { public int Value=42; }
        public static class Program {
            public static int Main() {
                var text=new ArgumentOutOfRangeException("name","invalid","unknown");
                Console.WriteLine(text.ParamName); Console.WriteLine((string)text.ActualValue); Console.WriteLine(text.Message);
                var number=new ArgumentOutOfRangeException("index",42,"bad");
                Console.WriteLine((int)number.ActualValue); Console.WriteLine(number.Message);
                var missing=new ArgumentOutOfRangeException(null,null,"bad");
                Console.WriteLine(missing.ActualValue==null); Console.WriteLine(missing.Message);
                var payload=new Payload(); var retained=new ArgumentOutOfRangeException("payload",payload,"bad");
                // Construction and ActualValue access must not try to format an arbitrary object.
                return retained.ActualValue==payload ? ((Payload)retained.ActualValue).Value : 1;
            }
        }
        """, "name\ninvalid\nunknown (Parameter 'name')\nActual value was invalid.\n42\nbad (Parameter 'index')\nActual value was 42.\nTrue\nbad\n", 42);

    [Fact]
    public Task ConstructorsAndCatchHierarchyMatchClr() => InlineArrayTests.AssertLinked("""
        using System;
        public static class Program {
            static void Print(ArgumentException error) {
                Console.WriteLine(error.ParamName); Console.WriteLine(error.Message);
            }
            public static int Main() {
                Print(new ArgumentOutOfRangeException());
                Print(new ArgumentOutOfRangeException("duration"));
                Print(new ArgumentOutOfRangeException("duration", "custom"));
                Print(new ArgumentOutOfRangeException("", "custom"));
                Print(new ArgumentOutOfRangeException(null, (string)null));
                Exception error = new ArgumentOutOfRangeException("duration");
                Console.WriteLine(error is ArgumentException);
                Console.WriteLine(error is SystemException);
                Console.WriteLine(error is ArgumentNullException);
                try { throw error; }
                catch (ArgumentNullException) { Console.WriteLine("wrong catch"); }
                catch (ArgumentOutOfRangeException caught) { Console.WriteLine(caught == error); }
                return 42;
            }
        }
        """, "\nSpecified argument was out of the range of valid values.\nduration\nSpecified argument was out of the range of valid values. (Parameter 'duration')\nduration\ncustom (Parameter 'duration')\n\ncustom\n\nSpecified argument was out of the range of valid values.\nTrue\nTrue\nFalse\nTrue\n", 42);
}
