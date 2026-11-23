using System.Globalization;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Xui.Compiler.XuiNet;
using Xui.Compiler.XuiNet.Layout;
using Xui.Compiler.XuiNet.DotNet.Op;
using Xunit;
using Model = Xui.Compiler.XuiNet.DotNet;
using OpCode = Xui.Compiler.XuiNet.DotNet.Op.OpCode;

namespace Xui.Compiler.Tests.XuiNet.DotNet;

// A test consumer of XuiNet: declarations are visited depth-first; reference edges are printed by name.
internal sealed class AssemblyMarkdown
{
    private static readonly Dictionary<OpCode, System.Reflection.Emit.OpCode> Opcodes = typeof(OpCodes)
        .GetFields(BindingFlags.Public | BindingFlags.Static)
        .Where(field => field.FieldType == typeof(System.Reflection.Emit.OpCode))
        .Select(field => (System.Reflection.Emit.OpCode)field.GetValue(null)!)
        .Where(opcode => Enum.IsDefined((OpCode)unchecked((ushort)opcode.Value)))
        .ToDictionary(opcode => (OpCode)unchecked((ushort)opcode.Value));
    private static readonly JsonSerializerOptions Json = new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
    private readonly Model.Module module;
    private readonly LayoutResolver layouts;
    private readonly StringBuilder text = new();
    private readonly Dictionary<TypeDefinitionHandle, TypeDefinitionHandle> parents = new();
    private readonly HashSet<TypeDefinitionHandle> visited = new();

    private AssemblyMarkdown(Model.Module module)
    {
        this.module = module;
        layouts = new(module, TargetLayout.Wasm32);
        foreach (var handle in module.Types)
            foreach (var child in module.GetType(handle).NestedTypes)
                parents.Add(child, handle);
    }

    public static string Write(Model.Assembly assembly, string fixture, string sourcePath)
    {
        var dump = new AssemblyMarkdown(assembly.ManifestModule);
        dump.Assembly(assembly, fixture, sourcePath);
        return dump.text.ToString().ReplaceLineEndings("\n");
    }

    private void Assembly(Model.Assembly assembly, string fixture, string sourcePath)
    {
        var identity = assembly.Identity;
        Heading(1, $"Assembly `{identity.Name}`");
        text.AppendLine($"Source: [{fixture}.cs]({sourcePath})").AppendLine();
        Property("Version", identity.Version.ToString());
        Property("Culture", identity.Culture.Length == 0 ? "neutral" : identity.Culture);
        Property("Flags", identity.Flags.ToString());
        Property("Hash algorithm", identity.HashAlgorithm.ToString());
        Property("Public key", Hex(identity.PublicKey));
        Heading(2, $"Module `{module.Name}`");
        Assert.NotEqual(Guid.Empty, module.Mvid);
        Property("MVID", "<normalized>");
        Property("Layout target", layouts.Target.Name);
        Property("Entry point", Entity(module.EntryPoint));
        Property("Declarations", $"{module.Types.Count} types, {module.Fields.Count} fields, {module.Methods.Count} methods");

        Heading(3, "Assembly references");
        foreach (var handle in module.AssemblyReferences)
        {
            var reference = module.GetAssemblyReference(handle);
            Heading(4, $"`{reference.Name}` [{Token(handle)}]");
            Property("Version", reference.Version.ToString());
            Property("Culture", reference.Culture.Length == 0 ? "neutral" : reference.Culture);
            Property("Flags", reference.Flags.ToString());
            Property("Public key or token", Hex(reference.PublicKeyOrToken));
            Property("Hash", Hex(reference.HashValue));
        }
        Heading(3, "Type references");
        foreach (var handle in module.TypeReferences)
        {
            var type = module.GetTypeReference(handle);
            Property(Token(handle), $"{Name(type.Namespace, type.Name)}; scope: {Entity(type.ResolutionScope)}");
        }
        Heading(3, "Member references");
        foreach (var handle in module.MemberReferences)
        {
            var member = module.GetMemberReference(handle);
            Property(Token(handle), $"{member.Kind} {member.Name}; parent: {Entity(member.Parent)}; signature: {Hex(member.Signature)}");
        }

        if (module.TypeSpecifications.Any())
        {
            Heading(3, "Type specifications");
            foreach (var handle in module.TypeSpecifications)
            {
                var specification = module.GetTypeSpecification(handle);
                Property(Token(handle), $"{specification.Read()}; signature: {Hex(specification.Signature)}");
            }
        }
        foreach (var handle in module.Types)
            if (!parents.ContainsKey(handle)) Type(handle, 3);
        Assert.Equal(module.Types.Count, visited.Count);

        Heading(3, "String literals");
        if (module.Strings.Count == 0) text.AppendLine("None.");
        foreach (var (handle, value) in module.Strings.OrderBy(pair => MetadataTokens.GetToken(pair.Key)))
            Property($"0x{MetadataTokens.GetToken(handle):x8}", Quote(value));
    }

    private void Type(TypeDefinitionHandle handle, int depth)
    {
        Assert.True(visited.Add(handle), $"Type {Token(handle)} was visited twice.");
        var type = module.GetType(handle);
        string kind = type.IsInterface ? "Interface" :
            type.BaseType.Kind == HandleKind.TypeReference && module.GetTypeReference((TypeReferenceHandle)type.BaseType).Name == "ValueType"
                ? "Struct" : "Class";
        if (type.BaseType.Kind == HandleKind.TypeReference && module.GetTypeReference((TypeReferenceHandle)type.BaseType).Name == "Enum") kind = "Enum";
        if (type.Name == "<Module>") kind = "Module type";
        Heading(depth, $"{kind} {Code(TypeName(handle))} [{Token(handle)}]");
        Property("Namespace", type.Namespace.Length == 0 ? "(empty)" : type.Namespace);
        Property("Attributes", type.Attributes.ToString());
        Property("Base type", Entity(type.BaseType));
        Property("Is interface", type.IsInterface ? "true" : "false");
        GenericParameters(type.GenericParameters);
        foreach (var implementation in type.Interfaces)
            Property($"Interface [{Token(implementation)}]", Entity(module.GetInterfaceType(implementation)));
        StructLayout? layout = null;
        if (kind is "Struct" or "Enum")
        {
            try { Property("Storage classification", layouts.Classify(handle).ToString()); }
            catch (NotSupportedException error) { Property("Storage classification", $"Unsupported: {error.Message}"); }
            if (type.DeclaredLayout.PackingSize != 0) Property("Packing", type.DeclaredLayout.PackingSize.ToString(CultureInfo.InvariantCulture));
            if (type.DeclaredLayout.Size != 0) Property("Declared size", type.DeclaredLayout.Size.ToString(CultureInfo.InvariantCulture));
            try { layout = layouts.Resolve(handle); }
            catch (NotSupportedException error) { Property("Layout", $"Unsupported: {error.Message}"); }
            if (layout is not null)
            {
                Property("Alignment", layout.Alignment.ToString(CultureInfo.InvariantCulture));
                Property("Size", layout.Size.ToString(CultureInfo.InvariantCulture));
            }
        }
        foreach (var fieldHandle in type.Fields)
        {
            var field = module.GetField(fieldHandle);
            Heading(depth + 1, $"Field `{field.Name}` [{Token(fieldHandle)}]");
            Property("Declaring type", Entity(field.DeclaringType));
            Property("Attributes", field.Attributes.ToString());
            Property("Signature", Hex(field.Signature));
            if (layout is not null && layout.Fields.TryGetValue(fieldHandle, out var fieldLayout))
            {
                Property("Offset", fieldLayout.Offset.ToString(CultureInfo.InvariantCulture));
                Property("Size", fieldLayout.Size.ToString(CultureInfo.InvariantCulture));
            }
        }
        foreach (var methodHandle in type.Methods) Method(methodHandle, depth + 1);
        foreach (var child in type.NestedTypes) Type(child, depth + 1);
    }

    private void Method(MethodDefinitionHandle handle, int depth)
    {
        var method = module.GetMethod(handle);
        Heading(depth, $"Method `{method.Name}` [{Token(handle)}]");
        Property("Declaring type", Entity(method.DeclaringType));
        Property("Attributes", method.Attributes.ToString());
        Property("Implementation", method.ImplementationAttributes.ToString());
        Property("Signature", Hex(method.Signature));
        GenericParameters(method.GenericParameters);
        var body = method.Body;
        if (body is null)
        {
            Property("Body", "none");
            return;
        }
        Property("RVA", $"0x{body.RelativeVirtualAddress:x8}");
        Property("Max stack", body.MaxStack.ToString(CultureInfo.InvariantCulture));
        Property("Init locals", body.InitLocals ? "true" : "false");
        Property("Local signature", Hex(body.LocalSignature));
        text.AppendLine().AppendLine("```il");
        var reader = body.Instructions;
        while (reader.HasRemaining)
        {
            int offset = reader.Offset;
            var instruction = reader.Read();
            if (!Opcodes.TryGetValue(instruction.Code, out var descriptor))
                throw new NotSupportedException($"Unknown opcode {instruction.Code} at IL_{offset:x4}.");
            string operand = Operand(instruction, descriptor.OperandType, ref reader);
            text.AppendLine($"IL_{offset:x4}: {descriptor.Name}{(operand.Length == 0 ? "" : " " + operand)}");
        }
        text.AppendLine("```");
        if (body.ExceptionRegions.Length == 0) return;
        text.AppendLine().AppendLine("Exception regions:").AppendLine();
        foreach (var region in body.ExceptionRegions)
        {
            string extra = region.Kind switch
            {
                ExceptionRegionKind.Catch => $"; catch: {Entity(region.CatchType)}",
                ExceptionRegionKind.Filter => $"; filter: IL_{region.FilterOffset:x4}",
                _ => ""
            };
            Property(region.Kind.ToString(), $"try [IL_{region.TryOffset:x4}, IL_{region.TryOffset + region.TryLength:x4}); " +
                $"handler [IL_{region.HandlerOffset:x4}, IL_{region.HandlerOffset + region.HandlerLength:x4}){extra}");
        }
    }

    private string Operand(Instruction instruction, OperandType type, ref ILReader reader)
    {
        switch (type)
        {
            case OperandType.InlineNone: return "";
            case OperandType.InlineI: return instruction.ReadInt(ref reader).Value.ToString(CultureInfo.InvariantCulture);
            case OperandType.ShortInlineI: return instruction.ReadShortInt(ref reader).Value.ToString(CultureInfo.InvariantCulture);
            case OperandType.InlineI8: return instruction.ReadLong(ref reader).Value.ToString(CultureInfo.InvariantCulture);
            case OperandType.ShortInlineR: return instruction.ReadFloat(ref reader).Value.ToString("R", CultureInfo.InvariantCulture);
            case OperandType.InlineR: return instruction.ReadDouble(ref reader).Value.ToString("R", CultureInfo.InvariantCulture);
            case OperandType.InlineVar: return instruction.ReadVariable(ref reader).Index.ToString(CultureInfo.InvariantCulture);
            case OperandType.ShortInlineVar: return instruction.ReadShortVariable(ref reader).Index.ToString(CultureInfo.InvariantCulture);
            case OperandType.InlineBrTarget: return $"IL_{instruction.ReadBranch(ref reader).Target:x4}";
            case OperandType.ShortInlineBrTarget: return $"IL_{instruction.ReadShortBranch(ref reader).Target:x4}";
            case OperandType.InlineSwitch:
                var branch = instruction.ReadSwitch(ref reader);
                var targets = new List<string>();
                foreach (int target in branch.Targets) targets.Add($"IL_{target:x4}");
                return "(" + string.Join(", ", targets) + ")";
            case OperandType.InlineString:
                int token = instruction.ReadToken(ref reader).Value;
                return $"{Quote(module.GetString(MetadataTokens.UserStringHandle(token & 0xffffff)))} [0x{token:x8}]";
            case OperandType.InlineField:
            case OperandType.InlineMethod:
            case OperandType.InlineSig:
            case OperandType.InlineTok:
            case OperandType.InlineType:
                return Entity(MetadataTokens.EntityHandle(instruction.ReadToken(ref reader).Value));
            default: throw new NotSupportedException($"Operand encoding {type} for {instruction.Code}.");
        }
    }

    private string Entity(EntityHandle handle)
    {
        if (handle.IsNil) return "none";
        string name;
        switch (handle.Kind)
        {
            case HandleKind.TypeDefinition: name = TypeName((TypeDefinitionHandle)handle); break;
            case HandleKind.TypeReference:
                var type = module.GetTypeReference((TypeReferenceHandle)handle);
                name = Name(type.Namespace, type.Name); break;
            case HandleKind.TypeSpecification:
                name = module.GetTypeSpecification((TypeSpecificationHandle)handle).Read().ToString(); break;
            case HandleKind.MethodDefinition:
                var method = module.GetMethod((MethodDefinitionHandle)handle);
                name = TypeName(method.DeclaringType) + "." + method.Name; break;
            case HandleKind.FieldDefinition:
                var field = module.GetField((FieldDefinitionHandle)handle);
                name = TypeName(field.DeclaringType) + "." + field.Name; break;
            case HandleKind.MemberReference:
                var member = module.GetMemberReference((MemberReferenceHandle)handle);
                name = Entity(member.Parent) + "." + member.Name; break;
            case HandleKind.AssemblyReference:
                name = module.GetAssemblyReference((AssemblyReferenceHandle)handle).Name; break;
            default: name = handle.Kind.ToString(); break;
        }
        return $"{name} [{Token(handle)}]";
    }

    private string TypeName(TypeDefinitionHandle handle)
    {
        var type = module.GetType(handle);
        return parents.TryGetValue(handle, out var parent)
            ? TypeName(parent) + "+" + type.Name : Name(type.Namespace, type.Name);
    }

    private void GenericParameters(GenericParameterHandleCollection parameters)
    {
        foreach (var handle in parameters)
        {
            var parameter = module.GetGenericParameter(handle);
            Property($"Generic parameter {parameter.Index}", $"{parameter.Name}; attributes: {parameter.Attributes}");
            foreach (var constraint in parameter.Constraints)
                Property($"Constraint [{Token(constraint)}]", Entity(module.GetGenericConstraintType(constraint)));
        }
    }

    private static string Name(string ns, string name) => ns.Length == 0 ? name : ns + "." + name;
    private static string Token(EntityHandle handle) => $"0x{MetadataTokens.GetToken(handle):x8}";
    private static string Quote(string value) => JsonSerializer.Serialize(value, Json);
    private static string Hex(BlobReader reader)
    {
        if (reader.Length == 0) return "(empty)";
        var result = new StringBuilder();
        while (reader.RemainingBytes > 0)
        {
            if (result.Length != 0) result.Append(' ');
            result.Append(reader.ReadByte().ToString("X2", CultureInfo.InvariantCulture));
        }
        return result.ToString();
    }

    private void Heading(int depth, string title)
    {
        if (text.Length > 0) text.AppendLine();
        text.Append('#', depth).Append(' ').AppendLine(title).AppendLine();
    }

    private void Property(string name, string value) => text.AppendLine($"- {name}: {Code(value)}");

    private static string Code(string value)
    {
        string delimiter = "`";
        while (value.Contains(delimiter, StringComparison.Ordinal)) delimiter += "`";
        return delimiter.Length == 1 ? delimiter + value + delimiter : delimiter + " " + value + " " + delimiter;
    }
}
