using Microsoft.CodeAnalysis;
using Xui.Compiler.Tests.Wasm.Gc;
using Xui.Compiler.Tests.Wasm.Repl;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Exceptions;

[Collection(nameof(ManagedConsoleCollection))]
public class EncodingTests
{
    [Theory]
    [InlineData(OptimizationLevel.Debug)]
    [InlineData(OptimizationLevel.Release)]
    public async Task FallbackExceptionsPreserveHierarchyMessagesAndInnerIdentity(OptimizationLevel optimization)
    {
        byte[] image=Frontend.Roslyn.Compile("""
            using System;
            using System.Text;
            public static class Program {
                static void Check(Exception error) {
                    Console.WriteLine(error.Message);
                    Console.WriteLine(error is ArgumentException);
                    Console.WriteLine(error is SystemException);
                    Console.WriteLine(error is EncoderFallbackException);
                    Console.WriteLine(error is DecoderFallbackException);
                    try { throw error; }
                    catch(ArgumentException caught) { Console.WriteLine(caught==error); Console.WriteLine(caught.ParamName==null); }
                }
                public static int Main() {
                    Check(new EncoderFallbackException()); Check(new DecoderFallbackException());
                    Check(new EncoderFallbackException((string)null)); Check(new DecoderFallbackException("decode"));
                    var inner=new Exception("inner");
                    var outer=new EncoderFallbackException("encode",inner);
                    Check(outer); Console.WriteLine(outer.InnerException==inner);
                    var empty=new DecoderFallbackException(null,(Exception)null);
                    Check(empty); Console.WriteLine(empty.InnerException==null);
                    try { throw outer; } catch(EncoderFallbackException caught) { Console.WriteLine(caught==outer); }
                    return 42;
                }
            }
            """,optimizationLevel:optimization);
        Assert.Equal(GcTests.RunManaged(image),await Node.ExecuteAsync(GcTests.Emit(image)));
    }
}
