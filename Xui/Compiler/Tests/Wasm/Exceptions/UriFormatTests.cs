using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Exceptions;

[Collection(nameof(ManagedConsoleCollection))]
public class UriFormatTests
{
    [Fact]
    public Task UriFailuresKeepFormatHierarchyMessagesAndInnerIdentity() => InlineArrayTests.AssertLinked("""
        using System;
        public static class Program {
            static void Raise(Exception error) => throw error;
            public static int Main() {
                var inner=new InvalidOperationException("inner");
                var error=new UriFormatException("bad URI",inner);
                try { Raise(error); } catch(UriFormatException e) {
                    Console.WriteLine(e.Message); Console.WriteLine(object.ReferenceEquals(inner,e.InnerException));
                }
                try { Raise(error); } catch(FormatException) { Console.WriteLine("format"); }
                try { Raise(error); } catch(SystemException) { Console.WriteLine("system"); }
                try { Raise(error); } catch(Exception) { Console.WriteLine("exception"); }
                Console.WriteLine(error is FormatException);
                Console.WriteLine(new UriFormatException().Message);
                Console.WriteLine(new UriFormatException((string)null).Message);
                return 42;
            }
        }
        """, "bad URI\nTrue\nformat\nsystem\nexception\nTrue\nOne of the identified items was in an invalid format.\nOne of the identified items was in an invalid format.\n",42,typeof(UriFormatException).Assembly);
}
