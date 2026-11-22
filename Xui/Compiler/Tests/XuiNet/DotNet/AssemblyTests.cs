using Xui.Compiler.Tests.Wasm.Rewrite;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using Microsoft.CodeAnalysis;
using Xui.Compiler.XuiNet;
using Xui.Compiler.XuiNet.DotNet.Op;
using Xui.Compiler.WASM.Emitter;
using Xunit;
using IR = Xui.Compiler.XuiNet.DotNet;

namespace Xui.Compiler.Tests.XuiNet.DotNet;

public class AssemblyTests
{
    [Fact]
    public void ReadsLibraryTypesAndMembersAfterInputIsDisposed()
    {
        using var assembly = Read("""
            using System.Reflection;
            [assembly: AssemblyVersion("1.2.3.4")]
            namespace Example;
            public interface IGreeter { string Greet(); }
            public class Greeter : IGreeter
            {
                public string Message = "Hello World";
                public string Greet() => Message;
                public class Nested { }
            }
            """, OutputKind.DynamicallyLinkedLibrary);
        Assert.Equal("HelloWorld", assembly.Identity.Name);
        Assert.Equal(new Version(1, 2, 3, 4), assembly.Identity.Version);
        Assert.Equal(0, assembly.Identity.PublicKey.Length);
        var module = assembly.ManifestModule;
        Assert.False(string.IsNullOrEmpty(module.Name));
        Assert.NotEqual(Guid.Empty, module.Mvid);
        Assert.True(module.EntryPoint.IsNil);
        var contract = module.GetType(Assert.Single(module.Types, handle => module.GetType(handle).Name == "IGreeter"));
        var greeter = module.GetType(Assert.Single(module.Types, handle => module.GetType(handle).Name == "Greeter"));
        Assert.True(contract.IsInterface);
        Assert.False(greeter.IsInterface);
        Assert.Equal("Example", greeter.Namespace);
        Assert.Equal((EntityHandle)contract.Handle, module.GetInterfaceType(Assert.Single(greeter.Interfaces)));
        var baseType = module.GetTypeReference((TypeReferenceHandle)greeter.BaseType);
        Assert.Equal(("System", "Object"), (baseType.Namespace, baseType.Name));
        Assert.Equal("Nested", module.GetType(Assert.Single(greeter.NestedTypes)).Name);
        var field = module.GetField(Assert.Single(greeter.Fields));
        Assert.Equal("Message", field.Name);
        Assert.Equal(greeter.Handle, field.DeclaringType);
        var fieldSignature = field.Signature;
        Assert.Equal(SignatureKind.Field, fieldSignature.ReadSignatureHeader().Kind);
        Assert.Equal(SignatureTypeCode.String, fieldSignature.ReadSignatureTypeCode());
        Assert.Equal(0, fieldSignature.RemainingBytes);
        Assert.Equal(0, field.Signature.Offset); // Each property access returns an independent cursor.
        var method = module.GetMethod(Assert.Single(greeter.Methods, handle => module.GetMethod(handle).Name == "Greet"));
        Assert.Equal(greeter.Handle, method.DeclaringType);
        Assert.True(method.Signature.Length > 0);
        int fieldToken = MetadataTokens.GetToken(field.Handle);
        var fieldReader = Find(method.Body!, OpCode.Ldfld);
        Assert.Equal(fieldToken, fieldReader.Read().ReadToken(ref fieldReader).Value);
        var abstractMethod = module.GetMethod(Assert.Single(contract.Methods));
        Assert.True((abstractMethod.Attributes & MethodAttributes.Abstract) != 0);
        Assert.Null(abstractMethod.Body);
        Assert.Empty(module.Strings);
        var constructor = module.GetMethod(Assert.Single(greeter.Methods, handle => module.GetMethod(handle).Name == ".ctor"));
        var literalReader = Find(constructor.Body!, OpCode.Ldstr);
        var literal = literalReader.Read().ReadToken(ref literalReader);
        Assert.Empty(module.Strings); // Iterating IL does not materialize strings.
        Assert.Equal("Hello World", module.GetString(MetadataTokens.UserStringHandle(literal.Value & 0xffffff)));
        Assert.Contains("Hello World", module.Strings.Values);
        var dependency = module.GetAssemblyReference((AssemblyReferenceHandle)baseType.ResolutionScope);
        Assert.Equal(typeof(object).Assembly.GetName().Name, dependency.Name);
        var token = dependency.PublicKeyOrToken;
        Assert.Equal(typeof(object).Assembly.GetName().GetPublicKeyToken(), token.ReadBytes(token.Length));
        Assert.Equal(0, dependency.PublicKeyOrToken.Offset);
        Assert.Equal(0, dependency.HashValue.Length);
        Assert.Throws<NotSupportedException>(() => new WasmEmitter().Emit(assembly));
    }

    [Fact]
    public void PreservesControlFlowConstantsAndExceptionRegions()
    {
        using var assembly = Read("""
            public static class Sample
            {
                public static bool Greater(int a, int b) => a > b;
                public static long Large() => 0x1122334455667788;
                public static double Real() => 1.25;
                public static float Single() => 1.5f;
                public static int Negative() => -17;
                public static int Fifth(int a, int b, int c, int d, int e) => e;
                public static string Choose(int value)
                {
                    try
                    {
                        switch (value)
                        {
                            case 0: return "zero";
                            case 1: return "one";
                            case 2: return "two";
                            case 3: return "three";
                            case 4: return "four";
                            default: return "other";
                        }
                    }
                    finally { System.Console.WriteLine("done"); }
                }
            }
            """, OutputKind.DynamicallyLinkedLibrary);
        var module = assembly.ManifestModule;
        IR.MethodBody Body(string name) => module.GetMethod(module.Methods.Single(handle => module.GetMethod(handle).Name == name)).Body!;
        var greater = Find(Body("Greater"), OpCode.Cgt);
        int opcodeOffset = greater.Offset;
        Assert.Equal(OpCode.Cgt, greater.Read().Code);
        Assert.Equal(opcodeOffset + 2, greater.Offset); // Two-byte opcode, no operand.
        var large = Find(Body("Large"), OpCode.Ldc_i8);
        opcodeOffset = large.Offset;
        var integer = large.Read();
        Assert.Equal(opcodeOffset + 1, large.Offset); // Read leaves all eight operand bytes untouched.
        Assert.Equal(0x1122334455667788L, integer.ReadLong(ref large).Value);
        Assert.Equal(opcodeOffset + 9, large.Offset);
        Assert.Equal(OpCode.Ret, large.Read().Code);
        var real = Find(Body("Real"), OpCode.Ldc_r8);
        Assert.Equal(1.25, real.Read().ReadDouble(ref real).Value);
        var single = Find(Body("Single"), OpCode.Ldc_r4);
        Assert.Equal(1.5f, single.Read().ReadFloat(ref single).Value);
        var negative = Find(Body("Negative"), OpCode.Ldc_i4_s);
        Assert.Equal(-17, negative.Read().ReadShortInt(ref negative).Value);
        var fifth = Find(Body("Fifth"), OpCode.Ldarg_s);
        Assert.Equal((ushort)4, fifth.Read().ReadShortVariable(ref fifth).Index);
        var body = Body("Choose");
        Assert.True(body.MaxStack > 0);
        Assert.True(body.InitLocals);
        Assert.True(body.LocalSignature.Length > 0);
        var localSignature = body.LocalSignature;
        Assert.Equal(SignatureKind.LocalVariables, localSignature.ReadSignatureHeader().Kind);
        Assert.Equal(ExceptionRegionKind.Finally, Assert.Single(body.ExceptionRegions).Kind);
        var switchReader = Find(body, OpCode.Switch);
        int switchOffset = switchReader.Offset;
        var branch = switchReader.Read().ReadSwitch(ref switchReader);
        Assert.Equal(1 + sizeof(int) * (1 + branch.Targets.Length), switchReader.Offset - switchOffset);
        Assert.Equal(switchReader.Offset, branch.Targets.BaseOffset);
        string[] expected = ["zero", "one", "two", "three", "four"];
        Assert.Equal(expected.Length, branch.Targets.Length);
        for (int i = 0; i < expected.Length; i++)
        {
            var targetReader = FindAt(body, branch.Targets[i]);
            var target = targetReader.Read();
            Assert.Equal(OpCode.Ldstr, target.Code);
            int token = target.ReadToken(ref targetReader).Value;
            Assert.Equal(expected[i], module.GetString(MetadataTokens.UserStringHandle(token & 0xffffff)));
        }
        var reader = body.Instructions;
        while (reader.HasRemaining)
        {
            var instruction = reader.Read();
            if (instruction.Code is OpCode.Leave or OpCode.Leave_s)
            {
                int target = instruction.Code == OpCode.Leave
                    ? instruction.ReadBranch(ref reader).Target
                    : instruction.ReadShortBranch(ref reader).Target;
                Assert.Equal(target, FindAt(body, target).Offset);
            }
            else Consume(instruction, ref reader);
        }

        long checksum = Scan(body); // Warm the method block before measuring.
        long before = GC.GetAllocatedBytesForCurrentThread();
        long total = 0;
        for (int i = 0; i < 100; i++) total += Scan(body);
        long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        Assert.Equal(checksum * 100, total);
        Assert.Equal(0, allocated);
    }

    [Theory]
    [InlineData("System.Console.Write(\"Hello World\");", "Write")]
    [InlineData("System.Console.WriteLine(\"{0}\", \"Hello World\");", "WriteLine")]
    public void PreservesCallsAndRejectsUnsupportedBackendInstructions(string source, string methodName)
    {
        using var assembly = Read(source);
        var module = assembly.ManifestModule;
        var main = module.GetMethod(module.EntryPoint);
        var callReader = Find(main.Body!, OpCode.Call);
        var call = callReader.Read().ReadToken(ref callReader);
        var member = module.GetMemberReference((MemberReferenceHandle)MetadataTokens.EntityHandle(call.Value));
        Assert.Equal(methodName, member.Name);
        Assert.Equal(MemberReferenceKind.Method, member.Kind);
        Assert.Contains("Call", Assert.Throws<NotSupportedException>(() => new WasmEmitter().Emit(assembly, new ConsoleWriteLineRewriter())).Message);
    }

    [Fact]
    public void RejectsUnknownOpcodeWhenIterated()
    {
        byte[] image = Frontend.Roslyn.Compile("System.Console.WriteLine(\"Hello World\");");
        using (var stream = new MemoryStream(image))
        using (var pe = new PEReader(stream))
        {
            var metadata = pe.GetMetadataReader();
            var entryPoint = (MethodDefinitionHandle)MetadataTokens.EntityHandle(
                pe.PEHeaders.CorHeader!.EntryPointTokenOrRelativeVirtualAddress);
            int rva = metadata.GetMethodDefinition(entryPoint).RelativeVirtualAddress;
            var section = pe.PEHeaders.SectionHeaders.Single(section =>
                rva >= section.VirtualAddress && rva < section.VirtualAddress + section.VirtualSize);
            int fileOffset = section.PointerToRawData + rva - section.VirtualAddress;
            Assert.Equal(2, image[fileOffset] & 3); // Tiny header: IL starts one byte later.
            image[fileOffset + 1] = 0xff;
        }
        using var input = new MemoryStream(image);
        using var assembly = AssemblyCompiler.Read(input);
        Assert.Contains("IL_0000", Assert.Throws<NotSupportedException>(() => new WasmEmitter().Emit(assembly)).Message);
    }

    [Fact]
    public void DefersReadingUnusedMethodBodies()
    {
        byte[] image = Frontend.Roslyn.Compile("""
            public static class Program
            {
                public static void Main() => System.Console.WriteLine("Hello World");
                public static void Unused() => System.Console.WriteLine("unused");
            }
            """);
        int rva;
        int fileOffset;
        using (var stream = new MemoryStream(image))
        using (var pe = new PEReader(stream))
        {
            var metadata = pe.GetMetadataReader();
            var method = metadata.MethodDefinitions.Select(metadata.GetMethodDefinition)
                .Single(method => metadata.GetString(method.Name) == "Unused");
            rva = method.RelativeVirtualAddress;
            var section = pe.PEHeaders.SectionHeaders.Single(section =>
                rva >= section.VirtualAddress && rva < section.VirtualAddress + section.VirtualSize);
            fileOffset = section.PointerToRawData + rva - section.VirtualAddress;
        }
        image[fileOffset] = 0; // Invalid body header: loading declarations must not read it.
        using var input = new MemoryStream(image);
        using var assembly = AssemblyCompiler.Read(input);
        var module = assembly.ManifestModule;
        var body = module.GetMethod(module.Methods.Single(handle => module.GetMethod(handle).Name == "Unused")).Body!;
        Assert.Equal(rva, body.RelativeVirtualAddress);
        Assert.Empty(assembly.ManifestModule.Strings);
        Assert.NotEmpty(new WasmEmitter().Emit(assembly, new ConsoleWriteLineRewriter()));
        Assert.DoesNotContain("unused", assembly.ManifestModule.Strings.Values);
        Assert.Throws<BadImageFormatException>(() => body.ExceptionRegions);
    }

    [Fact]
    public void AssemblyOwnsTheReaderUsedForDeferredBodies()
    {
        var assembly = Read("System.Console.WriteLine(\"Hello World\");");
        var body = assembly.ManifestModule.GetMethod(assembly.ManifestModule.EntryPoint).Body!;
        Assert.Equal(OpCode.Ldstr, body.Instructions.Read().Code); // Also check disposal with an already cached body block.
        assembly.Dispose();
        Assert.Throws<ObjectDisposedException>(() => { var reader = body.Instructions; reader.Read(); });
        Assert.Throws<ObjectDisposedException>(() => assembly.Identity.Name);
        Assert.Throws<ObjectDisposedException>(() => assembly.ManifestModule.Types);
    }

    private static ILReader Find(IR.MethodBody body, OpCode code)
    {
        var reader = body.Instructions;
        while (reader.HasRemaining)
        {
            var start = reader;
            var instruction = reader.Read();
            if (instruction.Code == code) return start;
            Consume(instruction, ref reader);
        }
        throw new Xunit.Sdk.XunitException($"Missing opcode {code}.");
    }

    private static ILReader FindAt(IR.MethodBody body, int offset)
    {
        var reader = body.Instructions;
        while (reader.HasRemaining)
        {
            if (reader.Offset == offset) return reader;
            Consume(reader.Read(), ref reader);
        }
        throw new Xunit.Sdk.XunitException($"Missing instruction at IL_{offset:x4}.");
    }

    private static long Scan(IR.MethodBody body)
    {
        long checksum = 0;
        var reader = body.Instructions;
        while (reader.HasRemaining)
        {
            checksum += reader.Offset;
            var instruction = reader.Read();
            if (instruction.Code == OpCode.Switch)
            {
                var branch = instruction.ReadSwitch(ref reader);
                foreach (int target in branch.Targets) checksum += target;
            }
            else Consume(instruction, ref reader);
        }
        return checksum;
    }

    // This test consumer chooses how to read the instructions emitted by its fixtures.
    private static void Consume(Instruction instruction, ref ILReader reader)
    {
        switch (instruction.Code)
        {
            case OpCode.Nop:
            case OpCode.Ret:
            case OpCode.Ldarg_0:
            case OpCode.Ldarg_1:
            case OpCode.Ldarg_2:
            case OpCode.Ldarg_3:
            case OpCode.Ldloc_0:
            case OpCode.Stloc_0:
            case OpCode.Cgt:
            case OpCode.Endfinally:
                break;
            case OpCode.Ldstr:
            case OpCode.Call:
            case OpCode.Ldfld:
            case OpCode.Stfld:
                _ = instruction.ReadToken(ref reader); break;
            case OpCode.Ldc_i4:
                _ = instruction.ReadInt(ref reader); break;
            case OpCode.Ldc_i4_s:
                _ = instruction.ReadShortInt(ref reader); break;
            case OpCode.Ldc_i8:
                _ = instruction.ReadLong(ref reader); break;
            case OpCode.Ldc_r4:
                _ = instruction.ReadFloat(ref reader); break;
            case OpCode.Ldc_r8:
                _ = instruction.ReadDouble(ref reader); break;
            case OpCode.Ldarg:
                _ = instruction.ReadVariable(ref reader); break;
            case OpCode.Ldarg_s:
                _ = instruction.ReadShortVariable(ref reader); break;
            case OpCode.Br:
            case OpCode.Leave:
                _ = instruction.ReadBranch(ref reader); break;
            case OpCode.Br_s:
            case OpCode.Leave_s:
                _ = instruction.ReadShortBranch(ref reader); break;
            case OpCode.Switch:
                _ = instruction.ReadSwitch(ref reader); break;
            default:
                throw new Xunit.Sdk.XunitException($"Unhandled fixture opcode {instruction.Code}.");
        }
    }

    private static IR.Assembly Read(string source, OutputKind outputKind = OutputKind.ConsoleApplication)
    {
        using var stream = new MemoryStream(Frontend.Roslyn.Compile(source, outputKind));
        return AssemblyCompiler.Read(stream);
    }
}
