using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Exceptions;

[Collection(nameof(ManagedConsoleCollection))]
public class MissingMemberTests
{
    [Fact]
    public Task MessagesAndNominalHierarchyMatchClr() => InlineArrayTests.AssertLinked("""
        using System;
        public static class Program {
            public static int Main() {
                Console.WriteLine(new MemberAccessException().Message);
                Console.WriteLine(new MissingMemberException().Message);
                Console.WriteLine(new MissingMethodException().Message);
                Console.WriteLine(new MissingFieldException().Message);
                Console.WriteLine(new MissingMethodException("factory failed").Message);
                Exception error = new MissingMethodException();
                if (error is not MissingMemberException || error is not MemberAccessException || error is not SystemException) return 1;
                if (error is MissingFieldException || error is ApplicationException) return 2;
                if (error as MissingMethodException != error) return 3;
                try { throw error; }
                catch (MissingFieldException) { throw new Exception("wrong catch"); }
                catch (MissingMemberException) { Console.WriteLine("member catch"); }
                try { throw new MissingFieldException(); }
                catch (MemberAccessException) { Console.WriteLine("access catch"); }
                try {
                    try { throw error; }
                    catch (MissingMethodException) { throw; }
                } catch (SystemException caught) { if (caught != error) throw new Exception("rethrow lost identity"); }
                return 42;
            }
        }
        """, "Cannot access member.\nAttempted to access a missing member.\nAttempted to access a missing method.\nAttempted to access a non-existing field.\nfactory failed\nmember catch\naccess catch\n", 42);
}
