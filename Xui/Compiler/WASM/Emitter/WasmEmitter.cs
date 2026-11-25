using System.Reflection.Metadata;
using Xui.Compiler.WASM.Emitter.Rewrite;
using Xui.Compiler.WASM.Wasm;
using Xui.Compiler.WASM.Wasm.Indices;
using Xui.Compiler.WASM.Wasm.Section;
using Xui.Compiler.WASM.Wasm.Section.TypeDefinitions;
using IR = Xui.Compiler.XuiNet.DotNet;
using Xui.Compiler.XuiNet.Reachability;

using static Xui.Compiler.WASM.Wasm.Section.TypeDefinitions.ValueTypeCode;
using static Xui.Compiler.WASM.Wasm.Section.TypeDefinitions.ValueType;

namespace Xui.Compiler.WASM.Emitter;

/// <summary>Builds the WebAssembly module layout and delegates MSIL translation to CodeEmitter.</summary>
public sealed partial class WasmEmitter
{
    /// <summary>The class descriptors from the most recent successful emission.</summary>
    public IReadOnlyList<Runtime.Type> Types { get; private set; } = [];

    /// <summary>The source dependency graph from the most recent successful emission.</summary>
    public DependencyGraph? Dependencies { get; private set; }

    /// <summary>Emits the managed entry point and its dependencies, with optional rewrites and additional preservation roots.</summary>
    /// <remarks>Additional roots preserve code. Named exports also root static methods with a host-callable ABI.</remarks>
    public byte[] Emit(IR.Assembly assembly, CodeRewriter? rewriter = null, IEnumerable<Root>? additionalRoots = null,
        IReadOnlyDictionary<string, IR.MethodIdentity>? namedExports = null)
    {
        var source = assembly.ManifestModule;
        if (source.EntryPoint.IsNil)
            throw new NotSupportedException("WASM emission currently requires a managed entry point.");
        rewriter ??= NoOpCodeRewriter.Instance;
        rewriter.BeginModule(source);
        var roots = new List<Root> { Root.EntryPoint(assembly) };
        if (additionalRoots is not null) roots.AddRange(additionalRoots);
        if (namedExports is not null)
            foreach (var (name, identity) in namedExports)
            {
                ValidateExport(name, identity);
                roots.Add(new Root.Method(identity));
            }
        var dependencies = Analyzer.Analyze(roots,
            (IR.MethodIdentity method, int offset, XuiNet.DotNet.Op.Instruction instruction, ref XuiNet.ILReader reader) =>
            {
                RequireLinkedModule(method.DeclaringType.Module);
                rewriter.UseSource(method.DeclaringType.Module);
                return rewriter.Collect(instruction, ref reader);
            });
        foreach (var type in dependencies.Nodes.OfType<Dependency.Type>()) RequireLinkedModule(type.Identity.Module);
        void RequireLinkedModule(IR.Module module)
        {
            if (module != source && (source.Compilation is null || module.Compilation != source.Compilation))
                throw new NotSupportedException("Only same-module roots or assemblies bound to the same compilation can be emitted together.");
        }
        var methods = new Dictionary<IR.MethodIdentity, MethodPlan>();
        var orderedMethods = new List<MethodPlan>();
        var literals = new List<string>();
        var seenLiterals = new HashSet<string>(StringComparer.Ordinal);
        var storage = new StorageModel(dependencies);
        storage.SelectStackTypes(dependencies);
        storage.CollectStatics(dependencies);
        var mainIdentity = IR.MethodIdentity.FromDefinition(source, source.EntryPoint);
        foreach (var identity in dependencies.Methods.Concat(dependencies.Nodes.OfType<Dependency.Signature>().Select(n => n.Identity)).Distinct())
        {
            try
            {
                var method = new MethodPlan(identity.DeclaringType.Module, identity, entryPoint: identity == mainIdentity, storage,
                    declarationOnly: !dependencies.Contains(new Dependency.Method(identity)));
                methods.Add(identity, method);
                orderedMethods.Add(method);
                if (method.IsDeclaration) continue;
                foreach (string literal in CodeEmitter.CollectRequirements(identity.DeclaringType.Module, method, dependencies))
                    if (seenLiterals.Add(literal)) literals.Add(literal);
            }
            catch (NotSupportedException error)
            {
                error.Data["Xui.Method"] ??= identity;
                error.Data["Xui.DependencyPath"] ??= string.Join("\n → ", dependencies.PathTo(new Dependency.Method(identity)));
                throw;
            }
        }
        var main = methods[mainIdentity];
        foreach (var layout in dependencies.Nodes.OfType<Dependency.Layout>())
            if (storage.IsGc(layout.Identity)) _ = storage.Shape(layout.Identity);
        storage.CollectArrayElementShapes();
        if (storage.ExceptionsEnabled)
            foreach (var kind in Enum.GetValues<XuiNet.DotNet.Exceptions.ExceptionKind>())
            {
                if (kind > XuiNet.DotNet.Exceptions.ExceptionKind.ArgumentException && !storage.ExtraExceptionKinds.Contains(kind)) continue;
                string message = XuiNet.DotNet.Exceptions.ExceptionTypes.Message(kind);
                if (seenLiterals.Add(message)) literals.Add(message);
            }
        if (storage.ArgumentExceptionsEnabled)
            foreach (string text in new[] { " (Parameter '", "')" })
                if (seenLiterals.Add(text)) literals.Add(text);
        foreach (var type in storage.Statics.Values.Where(type => type.Constructor is not null))
            foreach (string text in new[] { type.Name, type.FailureMessage })
                if (seenLiterals.Add(text)) literals.Add(text);
        foreach (var method in orderedMethods) method.AllocateTemporaries(methods);
        var literalIndices = new Dictionary<string, (DataIndex Data, GlobalIndex Global)>(StringComparer.Ordinal);
        using var module = new MemoryStream();
        var writer = new ModuleWriter(module);
        writer.Header().V1();
        TypeIndex utf16ArrayType;
        TypeIndex fromCharCodeArrayType;
        TypeIndex stringConcatType = default, stringLengthType = default, stringEqualsType = default;
        var types = writer.TypeDefinitionsSection();
        try
        {
            utf16ArrayType = types.ArrayType(PackedTypeCode.I16, Mutability.Mutable);
            fromCharCodeArrayType = types.FunctionType([RefNull(utf16ArrayType), I32, I32], NonNullExternRef);
            if (storage.ArgumentExceptionsEnabled || storage.StringConcatEnabled)
            {
                stringConcatType = types.FunctionType([ExternRef, ExternRef], NonNullExternRef);
            }
            if (storage.ArgumentExceptionsEnabled || storage.Interpolation.Enabled) stringLengthType = types.FunctionType([ExternRef], I32);
            if (storage.StringEqualityEnabled) stringEqualsType = types.FunctionType([ExternRef, ExternRef], I32);
            storage.Interpolation.DefineTypes(ref types);
            storage.MathImports.DefineTypes(ref types);
            storage.Formatting.DefineTypes(ref types);
            storage.Primitives.DefineTypes(ref types);
            storage.Clock.DefineTypes(ref types);
            rewriter.DefineTypes(ref types);
            storage.DefineTypes(ref types, utf16ArrayType);
            storage.DefineReferenceAddressTypes(ref types);
            storage.DefineDelegateTypes(ref types, methods);
            foreach (var method in orderedMethods)
            {
                var parameters = Array.ConvertAll(method.Signature.Parameters,
                    parameter => (Wasm.Section.TypeDefinitions.ValueType)parameter.WasmType);
                parameters = [.. parameters, .. Enumerable.Repeat((Wasm.Section.TypeDefinitions.ValueType)I32, method.Signature.ReferenceParameterCount)];
                if (method.Signature.HasResultAddress) parameters = [.. parameters, I32];
                method.Type = types.FunctionType(parameters, method.Signature.WasmResults);
                method.DefineBlockTypes(ref types);
            }
        }
        finally
        {
            types.Dispose();
        }
        FunctionIndex fromCharCodeArrayFunction;
        FunctionIndex? stringConcat = null, stringLength = null, stringEquals = null;
        uint importedFunctionCount;
        var imports = writer.ImportSection();
        try
        {
            rewriter.ImportFunctions(ref imports);
            fromCharCodeArrayFunction = imports.Function("wasm:js-string", "fromCharCodeArray", fromCharCodeArrayType);
            if (storage.ArgumentExceptionsEnabled || storage.StringConcatEnabled)
            {
                stringConcat = imports.Function("wasm:js-string", "concat", stringConcatType);
            }
            if (storage.ArgumentExceptionsEnabled || storage.Interpolation.Enabled) stringLength = imports.Function("wasm:js-string", "length", stringLengthType);
            if (storage.StringEqualityEnabled) stringEquals = imports.Function("wasm:js-string", "equals", stringEqualsType);
            storage.Interpolation.Import(ref imports);
            storage.MathImports.Import(ref imports);
            storage.Formatting.Import(ref imports);
            storage.Primitives.Import(ref imports);
            storage.Clock.Import(ref imports);
            importedFunctionCount = imports.FunctionCount;
        }
        finally
        {
            imports.Dispose();
        }
        bool hasFrames = orderedMethods.Any(method => method.Frame is not null);
        GlobalIndex? stackPointer = null, referenceStackPointer = null;
        TableIndex? objectReferences = null, stringReferences = null;
        var functions = writer.FunctionSection(importedFunctionCount);
        try
        {
            foreach (var method in orderedMethods.Where(method => !method.IsDeclaration || method.IsDelegateInvoke)) method.Function = functions.Function(method.Type);
            foreach (var accessorType in storage.ReferenceAccessorTypes) storage.ReferenceAccessorFunctions.Add(functions.Function(accessorType));
            if (storage.Classes.Types.Count != 0) storage.InstanceTest = functions.Function(storage.InstanceTestType);
            foreach (var target in storage.DelegateTargets.Values) target.Function = functions.Function(target.Type);
            foreach (var helper in storage.DelegateFunctions.Values) helper.Function = functions.Function(helper.Type);
            storage.Interpolation.Declare(ref functions);
            storage.Formatting.Declare(ref functions);
            foreach (var type in storage.Statics.Values.Where(type => type.Constructor is not null))
                type.Ensure = functions.Function(storage.StaticInitializerType);
        }
        finally { functions.Dispose(); }
        if (storage.TableSize != 0 || storage.ReferenceSlotsEnabled)
        {
            using var table = writer.TableSection();
            if (storage.TableSize != 0) table.Functions(storage.TableSize);
            if (storage.ReferenceSlotsEnabled)
            {
                objectReferences = table.References(EqRef, StorageModel.ReferenceCapacity);
                stringReferences = table.References(ExternRef, StorageModel.ReferenceCapacity);
            }
        }
        if (hasFrames || storage.HasScalarAddressAccessors || storage.HasAddressProjection || storage.StaticData.Count != 0)
        {
            using var memory = writer.MemorySection();
            memory.Memory(storage.StaticData.MemoryPages, storage.StaticData.MemoryPages);
        }
        if (storage.ExceptionsEnabled)
        {
            using var tags = writer.TagSection();
            storage.ExceptionTag = tags.Exception(storage.ExceptionTagType);
        }
        var globals = writer.GlobalSection();
        try
        {
            if (storage.DelegatesEnabled) storage.DelegateMethodIds = globals.I32Array(storage.DelegateMethodsType, storage.DelegateTableMethods(methods));
            for (int i = 0; i < literals.Count; i++)
                literalIndices.Add(literals[i], (new DataIndex((uint)i), globals.RefNull(ExternRef, Mutability.Mutable)));
            if (hasFrames) stackPointer = globals.I32(MemoryFrame.Start, Mutability.Mutable);
            if (storage.ReferenceSlotsEnabled) referenceStackPointer = globals.I32(0, Mutability.Mutable);
            foreach (var comparer in storage.DefaultComparers.Keys.ToArray())
            {
                using var value = globals.Struct(storage.Shape(comparer).Type);
                storage.DefaultComparers[comparer] = value.Index;
            }
            foreach (var type in storage.Classes.Types)
            {
                using var descriptor = globals.Struct(storage.DescriptorType);
                type.Descriptor = descriptor.Index;
                descriptor.I32((int)type.Id);
                if (type.BaseType is { } parent) descriptor.GlobalGet(parent.Descriptor);
                else descriptor.RefNull(EqRef);
                descriptor.I32((int)type.TableStart).Utf16Array(utf16ArrayType, type.Name)
                    .I32(type.VirtualSlots.Count).I32((int)type.FieldCount);
                if (storage.ManagedTypesEnabled)
                    descriptor.Utf16Array(utf16ArrayType, type.Identity.Module.GetType((TypeDefinitionHandle)type.Identity.Handle).Name);
            }
            storage.DefineStaticGlobals(ref globals);
        }
        finally { globals.Dispose(); }
        // Frame state follows the string-cache globals in the same index space.
        var runtime = new EmissionRuntime(utf16ArrayType, fromCharCodeArrayFunction, stackPointer, referenceStackPointer, objectReferences, stringReferences, stringConcat, stringLength, stringEquals);
        using (var exports = writer.ExportSection())
        {
            exports.Function("main", main.Function);
            if (namedExports is not null)
                foreach (var (name, identity) in namedExports)
                    exports.Function(name, methods[identity].Function);
        }
        if (storage.TableSize != 0)
        {
            using var elements = writer.ElementSection();
            foreach (var target in storage.DelegateTargets.Values) elements.Functions(target.TableSlot, [target.Function]);
            foreach (var type in storage.Classes.Types)
            {
                foreach (var slot in type.VirtualSlots)
                    if (methods.TryGetValue(slot.Implementation, out var implementation) && !implementation.IsDeclaration)
                        elements.Functions(type.TableStart + (uint)storage.Classes.InterfaceMethods.Count + slot.Index, [implementation.Function]);
                foreach (var slot in type.InterfaceSlots)
                    if (methods.TryGetValue(slot.Implementation, out var implementation) && !implementation.IsDeclaration)
                    {
                        InterfaceAbi.Validate(methods[slot.Declaration], implementation);
                        elements.Functions(type.TableStart + slot.Index, [implementation.Function]);
                    }
            }
        }
        writer.DataCount(checked((uint)literals.Count + storage.StaticData.Count));
        var code = writer.CodeSection();
        try
        {
            foreach (var method in orderedMethods.Where(method => !method.IsDeclaration || method.IsDelegateInvoke))
            {
                if (method.IsDelegateInvoke)
                {
                    using var invoke = code.FunctionBody(storage.DelegateInvokeLocals(method));
                    storage.EmitDelegateInvoke(invoke, method);
                    continue;
                }
                using var bodyWriter = code.FunctionBody(method.LocalTypes);
                rewriter.UseSource(method.Identity.DeclaringType.Module);
                var emitter = new CodeEmitter(method.Identity.DeclaringType.Module, bodyWriter, literalIndices, runtime, rewriter, method, methods);
                emitter.Emit(method.Body.Instructions);
            }
            for (int i = 0; i < storage.ReferenceAccessorFunctions.Count; i++)
            {
                using var body = code.FunctionBody();
                storage.EmitReferenceAccessor(body, i, runtime, literalIndices);
            }
            if (storage.Classes.Types.Count != 0)
            {
                using var body = code.FunctionBody([EqRef]);
                Runtime.InstanceTest.Emit(body, storage.ClassRoot, storage.DescriptorType, storage.Classes.Types);
            }
            foreach (var target in storage.DelegateTargets.Values)
            {
                using var body = code.FunctionBody();
                StorageModel.EmitDelegateAdapter(body, target);
            }
            foreach (var (kind, helper) in storage.DelegateFunctions)
            {
                using var body = code.FunctionBody(helper.Locals);
                storage.EmitDelegateHelper(body, kind);
            }
            storage.Interpolation.Emit(ref code, runtime);
            if (storage.Formatting.Composite)
            {
                using var body = code.FunctionBody();
                storage.Formatting.EmitBoxValue(body, storage);
            }
            foreach (var type in storage.Statics.Values.Where(type => type.Constructor is not null))
            {
                using var body = code.FunctionBody([EqRef]);
                storage.EmitStaticInitializer(body, type, methods[type.Constructor!.Value], runtime, literalIndices);
            }
        }
        finally { code.Dispose(); }
        var data = writer.DataSection();
        try
        {
            foreach (string literal in literals)
                data.PassiveUtf16(literal);
            storage.StaticData.Write(ref data);
        }
        finally { data.Dispose(); }
        Types = Array.AsReadOnly(storage.Classes.Types.ToArray());
        Dependencies = dependencies;
        return module.ToArray();
    }
}
