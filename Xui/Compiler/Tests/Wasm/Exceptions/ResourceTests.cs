using Microsoft.CodeAnalysis;
using Xui.Compiler.Tests.Wasm.Gc;
using Xui.Compiler.Tests.Wasm.Repl;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Exceptions;

[Collection(nameof(ManagedConsoleCollection))]
public class ResourceTests
{
    [Theory]
    [InlineData(OptimizationLevel.Debug)]
    [InlineData(OptimizationLevel.Release)]
    public async Task MissingManifestResourcePreservesCatchHierarchyAndInnerException(OptimizationLevel optimization)
    {
        byte[] image = Frontend.Roslyn.Compile("""
            using System;
            using System.Resources;
            public static class Program {
                static void Check(Exception error) {
                    Console.WriteLine(error.Message);
                    Console.WriteLine(error is MissingManifestResourceException);
                    Console.WriteLine(error is SystemException);
                    Console.WriteLine(error is ApplicationException);
                    try { throw error; }
                    catch (MissingManifestResourceException caught) { Console.WriteLine(caught == error); }
                    try { throw error; }
                    catch (SystemException caught) { Console.WriteLine(caught == error); }
                }
                public static int Main() {
                    Check(new MissingManifestResourceException());
                    Check(new MissingManifestResourceException((string)null));
                    Check(new MissingManifestResourceException("missing"));
                    var inner = new Exception("inner");
                    var outer = new MissingManifestResourceException("resource", inner);
                    Check(outer); Console.WriteLine(outer.InnerException == inner);
                    var empty = new MissingManifestResourceException(null, null);
                    Check(empty); Console.WriteLine(empty.InnerException == null);
                    return 42;
                }
            }
            """, optimizationLevel: optimization);
        Assert.Equal(GcTests.RunManaged(image), await Node.ExecuteAsync(GcTests.Emit(image)));
    }
}
