using Microsoft.CodeAnalysis;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.Tests.Wasm.Canvas;
using Xui.Compiler.Tests.Wasm.Gc;
using Xui.Compiler.Tests.Wasm.Repl;
using Xui.Compiler.Tests.Wasm.Structs;
using Xui.Compiler.Playground;
using Xui.Compiler.WASM.Emitter;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Exceptions;

[Collection(nameof(ManagedConsoleCollection))]
public class XmlTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task InternalResourceConstructorsMatchClrFormatting(bool array, bool missingArguments)
    {
        string format = missingArguments ? "literal {0}" : "bad '{0}' {{XML}}";
        string[]? arguments = missingArguments ? null : ["tag"];
        var parameterType = array ? typeof(string[]) : typeof(string);
        var constructor = typeof(System.Xml.XmlException).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic,
            null, [typeof(string), parameterType, typeof(int), typeof(int), typeof(string)], null)!;
        var expected = (System.Xml.XmlException)constructor.Invoke([format, array ? arguments : "tag", 7, 9, "asset://floor.svg"]);
        string resource = (string)typeof(System.Xml.XmlException).GetProperty("ResString", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(expected)!;
        MemberReferenceHandle create = default, message = default, line = default, position = default, uri = default, res = default, printText = default, printInt = default;
        TypeReferenceHandle stringType = default;
        UserStringHandle formatText = default, argumentText = default, sourceUri = default;
        byte[] image = Frontend.Msil.Compile(il =>
        {
            il.LoadString(formatText);
            if (array)
            {
                if (missingArguments) il.OpCode(ILOpCode.Ldnull);
                else
                {
                    il.LoadConstantI4(1); il.OpCode(ILOpCode.Newarr); il.Token(stringType);
                    il.OpCode(ILOpCode.Dup); il.LoadConstantI4(0); il.LoadString(argumentText); il.OpCode(ILOpCode.Stelem_ref);
                }
            }
            else il.LoadString(argumentText);
            il.LoadConstantI4(7); il.LoadConstantI4(9); il.LoadString(sourceUri);
            il.OpCode(ILOpCode.Newobj); il.Token(create);
            foreach (var (getter, print) in new[] { (message, printText), (line, printInt), (position, printInt), (uri, printText), (res, printText) })
            {
                il.OpCode(ILOpCode.Dup); il.OpCode(ILOpCode.Callvirt); il.Token(getter); il.Call(print);
            }
            il.OpCode(ILOpCode.Pop); il.LoadConstantI4(42); il.OpCode(ILOpCode.Ret);
        }, configureMetadata: metadata =>
        {
            var xmlAssembly = typeof(System.Xml.XmlException).Assembly.GetName();
            var consoleAssembly = typeof(Console).Assembly.GetName();
            AssemblyReferenceHandle Assembly(AssemblyName name) => metadata.AddAssemblyReference(metadata.GetOrAddString(name.Name!), name.Version!, default,
                metadata.GetOrAddBlob(name.GetPublicKeyToken()!), 0, default);
            var xml = metadata.AddTypeReference(Assembly(xmlAssembly), metadata.GetOrAddString("System.Xml"), metadata.GetOrAddString("XmlException"));
            var console = metadata.AddTypeReference(Assembly(consoleAssembly), metadata.GetOrAddString("System"), metadata.GetOrAddString("Console"));
            stringType = metadata.AddTypeReference(MetadataTokens.AssemblyReferenceHandle(1), metadata.GetOrAddString("System"), metadata.GetOrAddString("String"));
            formatText = metadata.GetOrAddUserString(format); argumentText = metadata.GetOrAddUserString("tag"); sourceUri = metadata.GetOrAddUserString("asset://floor.svg");
            var signature = new BlobBuilder();
            new BlobEncoder(signature).MethodSignature(isInstanceMethod: true).Parameters(5, r => r.Void(), p => {
                p.AddParameter().Type().String();
                if (array) p.AddParameter().Type().SZArray().String(); else p.AddParameter().Type().String();
                p.AddParameter().Type().Int32(); p.AddParameter().Type().Int32(); p.AddParameter().Type().String();
            });
            create = metadata.AddMemberReference(xml, metadata.GetOrAddString(".ctor"), metadata.GetOrAddBlob(signature));
            MemberReferenceHandle Getter(string name, bool integer = false)
            {
                var blob = new BlobBuilder();
                new BlobEncoder(blob).MethodSignature(isInstanceMethod: true).Parameters(0, r => { if (integer) r.Type().Int32(); else r.Type().String(); }, _ => { });
                return metadata.AddMemberReference(xml, metadata.GetOrAddString(name), metadata.GetOrAddBlob(blob));
            }
            MemberReferenceHandle Print(bool integer)
            {
                var blob = new BlobBuilder();
                new BlobEncoder(blob).MethodSignature().Parameters(1, r => r.Void(), p => { if (integer) p.AddParameter().Type().Int32(); else p.AddParameter().Type().String(); });
                return metadata.AddMemberReference(console, metadata.GetOrAddString("WriteLine"), metadata.GetOrAddBlob(blob));
            }
            message = Getter("get_Message"); line = Getter("get_LineNumber", true); position = Getter("get_LinePosition", true);
            uri = Getter("get_SourceUri"); res = Getter("get_ResString"); printText = Print(false); printInt = Print(true);
        });
        using var app = CanvasTests.Read(image);
        using var framework = new Frontend.Framework("System.Private.Xml.dll");
        _ = new Xui.Compiler.XuiNet.Compilation([app, .. framework.Assemblies]);
        var actual = await Node.ExecuteAsync(new WasmEmitter().Emit(app, new ConsoleWriteLineRewriter()));
        Assert.Equal($"{expected.Message}\n{expected.LineNumber}\n{expected.LinePosition}\n{expected.SourceUri}\n{resource}\n", actual.Output);
        Assert.Equal(42, actual.ExitCode);
    }

    [Theory]
    [InlineData(OptimizationLevel.Debug)]
    [InlineData(OptimizationLevel.Release)]
    public async Task XmlErrorsPreservePositionsMessagesInnerIdentityAndPropagation(OptimizationLevel optimization)
    {
        byte[] image = Frontend.Roslyn.Compile("""
            using System;
            using System.Xml;
            using System.Reflection;
            public class Broken {
                public static int Value = Fail();
                static int Fail() { throw new XmlException("initializer", null, 2, 3); }
            }
            public class FailingConstructor {
                public FailingConstructor() { throw new XmlException("constructor", null, 4, 5); }
            }
            public static class Program {
                static void Check(XmlException error) {
                    Console.WriteLine(error.Message);
                    Console.WriteLine(((Exception)error).Message);
                    Console.WriteLine(error.LineNumber); Console.WriteLine(error.LinePosition);
                    Console.WriteLine(error.SourceUri == null);
                    try { try { throw error; } catch (XmlException) { throw; } }
                    catch (SystemException caught) { Console.WriteLine(caught == error); Console.WriteLine(caught is XmlException); }
                    finally { Console.WriteLine("finally"); }
                }
                public static int Main() {
                    Check(new XmlException()); Check(new XmlException(null)); Check(new XmlException("message"));
                    var inner = new InvalidOperationException("inner");
                    var outer = new XmlException("bad XML", inner, 12, 34);
                    Check(outer); Console.WriteLine(outer.InnerException == inner); Console.WriteLine(inner.Message);
                    Check(new XmlException(null, null, 3, 4));
                    Check(new XmlException("zero", null, 0, 7));
                    Check(new XmlException("negative", null, -1, -2));
                    try { ((XmlException)null).LineNumber.ToString(); }
                    catch (NullReferenceException e) { Console.WriteLine(e is XmlException); }
                    try { Console.WriteLine(Broken.Value); }
                    catch (TypeInitializationException e) { Check((XmlException)e.InnerException); Console.WriteLine(e.TypeName); }
                    try { Activator.CreateInstance<FailingConstructor>(); }
                    catch (TargetInvocationException e) { Check((XmlException)e.InnerException); }
                    return 42;
                }
            }
            """, optimizationLevel: optimization,
            additionalReferences: [MetadataReference.CreateFromFile(typeof(System.Xml.XmlException).Assembly.Location)]);
        var expected = GcTests.RunManaged(image);
        using var app = CanvasTests.Read(image);
        using var framework = new Frontend.Framework("System.Private.Xml.dll");
        _ = new Xui.Compiler.XuiNet.Compilation([app, .. framework.Assemblies]);
        Assert.Equal(expected, await Node.ExecuteAsync(new WasmEmitter().Emit(app, new ConsoleWriteLineRewriter())));
    }
}
