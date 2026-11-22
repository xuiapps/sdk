using System.Reflection.Metadata;
using Xui.Compiler.WASM.Emitter.Rewrite;
using Xui.Compiler.WASM.Wasm;
using Xui.Compiler.WASM.Wasm.Indices;
using Xui.Compiler.WASM.Wasm.Section;
using Xui.Compiler.WASM.Wasm.Section.TypeDefinitions;
using IR = Xui.Compiler.XuiNet.DotNet;

using static Xui.Compiler.WASM.Wasm.Section.TypeDefinitions.ValueTypeCode;
using static Xui.Compiler.WASM.Wasm.Section.TypeDefinitions.ValueType;

namespace Xui.Compiler.WASM.Emitter;

/// <summary>Builds the WebAssembly module layout and delegates MSIL translation to CodeEmitter.</summary>
public sealed class WasmEmitter
{
    /// <summary>The class descriptors from the most recent successful emission.</summary>
    public IReadOnlyList<Runtime.Type> Types { get; private set; } = [];

    /// <summary>Emits a module, optionally applying a caller-supplied instruction rewriter.</summary>
    public byte[] Emit(IR.Assembly assembly, CodeRewriter? rewriter = null)
    {
        var source = assembly.ManifestModule;
        if (source.EntryPoint.IsNil)
            throw new NotSupportedException("WASM emission currently requires a managed entry point.");
        foreach (var typeHandle in source.Types)
        {
            var type = source.GetType(typeHandle);
            if (type.Name != "<Module>") continue;
            foreach (var methodHandle in type.Methods)
                if (source.GetMethod(methodHandle).Name == ".cctor")
                    throw new NotSupportedException("Module initialization is not implemented.");
        }
        rewriter ??= NoOpCodeRewriter.Instance;
        rewriter.BeginModule(source);
        var methods = new Dictionary<IR.MethodIdentity, MethodPlan>();
        var orderedMethods = new List<MethodPlan>();
        var literals = new List<string>();
        var seenLiterals = new HashSet<string>(StringComparer.Ordinal);
        var storage = new StorageModel(source);
        var main = new MethodPlan(source, IR.MethodIdentity.FromDefinition(source, source.EntryPoint), entryPoint: true, storage);
        methods.Add(main.Identity, main);
        orderedMethods.Add(main);
        // Only read reachable bodies. Register before traversing calls so recursive edges terminate.
        for (int i = 0; i < orderedMethods.Count; i++)
        {
            var method = orderedMethods[i];
            if (method.IsAbstract) continue;
            foreach (string literal in CodeEmitter.CollectRequirements(source, method, rewriter))
                if (seenLiterals.Add(literal)) literals.Add(literal);
            foreach (var handle in method.Calls.Concat(storage.Classes.RequiredImplementations()).Distinct().ToArray())
            {
                if (methods.ContainsKey(handle)) continue;
                var callee = new MethodPlan(source, handle, entryPoint: false, storage);
                methods.Add(handle, callee);
                orderedMethods.Add(callee);
            }
        }
        foreach (var method in orderedMethods) method.AllocateTemporaries(methods);
        var literalIndices = new Dictionary<string, (DataIndex Data, GlobalIndex Global)>(StringComparer.Ordinal);
        using var module = new MemoryStream();
        var writer = new ModuleWriter(module);
        writer.Header().V1();
        TypeIndex utf16ArrayType;
        TypeIndex fromCharCodeArrayType;
        var types = writer.TypeDefinitionsSection();
        try
        {
            utf16ArrayType = types.ArrayType(PackedTypeCode.I16, Mutability.Mutable);
            fromCharCodeArrayType = types.FunctionType([RefNull(utf16ArrayType), I32, I32], NonNullExternRef);
            rewriter.DefineTypes(ref types);
            storage.DefineTypes(ref types, utf16ArrayType);
            foreach (var method in orderedMethods)
            {
                var parameters = Array.ConvertAll(method.Signature.Parameters,
                    parameter => (Wasm.Section.TypeDefinitions.ValueType)parameter.WasmType);
                Wasm.Section.TypeDefinitions.ValueType? result = method.Signature.ReturnType == SignatureTypeCode.Void
                    ? null : method.Signature.Result.WasmType;
                method.Type = types.FunctionType(parameters, result);
                method.DefineBlockTypes(ref types);
            }
        }
        finally
        {
            types.Dispose();
        }
        FunctionIndex fromCharCodeArrayFunction;
        uint importedFunctionCount;
        var imports = writer.ImportSection();
        try
        {
            rewriter.ImportFunctions(ref imports);
            fromCharCodeArrayFunction = imports.Function("wasm:js-string", "fromCharCodeArray", fromCharCodeArrayType);
            importedFunctionCount = imports.FunctionCount;
        }
        finally
        {
            imports.Dispose();
        }
        bool hasFrames = orderedMethods.Any(method => method.Frame is not null);
        GlobalIndex? stackPointer = null;
        using (var functions = writer.FunctionSection(importedFunctionCount))
        {
            foreach (var method in orderedMethods.Where(method => !method.IsAbstract)) method.Function = functions.Function(method.Type);
            if (storage.Classes.Types.Count != 0) storage.InstanceTest = functions.Function(storage.InstanceTestType);
        }
        if (storage.TableSize != 0)
        {
            using var table = writer.TableSection();
            table.Functions(storage.TableSize);
        }
        if (hasFrames)
        {
            using var memory = writer.MemorySection();
            memory.Memory(MemoryFrame.PageCount, MemoryFrame.PageCount);
        }
        using (var globals = writer.GlobalSection())
        {
            for (int i = 0; i < literals.Count; i++)
                literalIndices.Add(literals[i], (new DataIndex((uint)i), globals.RefNull(ExternRef, Mutability.Mutable)));
            if (hasFrames) stackPointer = globals.I32(MemoryFrame.Start, Mutability.Mutable);
            foreach (var type in storage.Classes.Types)
            {
                using var descriptor = globals.Struct(storage.DescriptorType);
                type.Descriptor = descriptor.Index;
                descriptor.I32((int)type.Id);
                if (type.BaseType is { } parent) descriptor.GlobalGet(parent.Descriptor);
                else descriptor.RefNull(EqRef);
                descriptor.I32((int)type.TableStart).Utf16Array(utf16ArrayType, type.Name)
                    .I32(type.VirtualSlots.Count).I32((int)type.FieldCount);
            }
        }
        // Frame state follows the string-cache globals in the same index space.
        var runtime = new EmissionRuntime(utf16ArrayType, fromCharCodeArrayFunction, stackPointer);
        using (var exports = writer.ExportSection())
        {
            exports.Function("main", main.Function);
        }
        if (storage.TableSize != 0)
        {
            using var elements = writer.ElementSection();
            foreach (var type in storage.Classes.Types)
                foreach (var slot in type.VirtualSlots)
                    if (methods.TryGetValue(slot.Implementation, out var implementation) && !implementation.IsAbstract)
                        elements.Functions(type.TableStart + slot.Index, [implementation.Function]);
        }
        writer.DataCount((uint)literals.Count);
        using (var code = writer.CodeSection())
        {
            foreach (var method in orderedMethods.Where(method => !method.IsAbstract))
            {
                using var bodyWriter = code.FunctionBody(method.LocalTypes);
                var emitter = new CodeEmitter(source, bodyWriter, literalIndices, runtime, rewriter, method, methods);
                emitter.Emit(method.Body.Instructions);
            }
            if (storage.Classes.Types.Count != 0)
            {
                using var body = code.FunctionBody([EqRef]);
                Runtime.InstanceTest.Emit(body, storage.ClassRoot, storage.DescriptorType);
            }
        }
        using (var data = writer.DataSection())
        {
            foreach (string literal in literals)
                data.PassiveUtf16(literal);
        }
        Types = Array.AsReadOnly(storage.Classes.Types.ToArray());
        return module.ToArray();
    }
}
