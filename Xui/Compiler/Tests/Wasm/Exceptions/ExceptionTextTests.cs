using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Exceptions;

[Collection(nameof(ManagedConsoleCollection))]
public class ExceptionTextTests
{
    [Fact]
    public Task DirectAndObjectCallsPreserveMessagesAndInnerChains()
    {
        const string source = """
            using System;
            public static class Program {
                static string Text(object value) => value.ToString();
                public static int Main() {
                    Exception[] errors = {
                        new PlatformNotSupportedException(), new PlatformNotSupportedException("platform", new Exception("inner")),
                        new Exception(), new Exception(""), new Exception("hello"),
                        new ArgumentNullException("value"), new ArgumentOutOfRangeException("index", 42, "outside"),
                        new InvalidOperationException("outer", new Exception("inner")),
                        new AggregateException("outer", new AggregateException("middle", new ArgumentException("inner"))),
                        new System.IO.EndOfStreamException("eof"), new System.Resources.MissingManifestResourceException("missing"),
                        new System.Xml.XmlException("xml", new Exception("inner"), 2, 3)
                    };
                    foreach (var error in errors) { Console.WriteLine(error.ToString()); Console.WriteLine(Text(error)); }
                    try { Text(null); } catch(NullReferenceException) { Console.WriteLine("null"); }
                    try { throw new PlatformNotSupportedException(); } catch(NotSupportedException) { Console.WriteLine("platform catch"); }
                    return 42;
                }
            }
            """;
        byte[] pe = Frontend.Roslyn.Compile(source, additionalReferences:
            [Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(typeof(System.Xml.XmlException).Assembly.Location)]);
        string expected = Gc.GcTests.RunManaged(pe).Output;
        return InlineArrayTests.AssertLinked(source, expected, 42, typeof(System.Xml.XmlException).Assembly);
    }
}
