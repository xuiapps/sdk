using System.Globalization;
using System.Text;
using Xui.GPU.IR;

namespace Xui.GPU.Backends.Glsl;

/// <summary>Emits GLSL ES 3.00 for WebGL 2 from the shared shader IR.</summary>
/// <remarks>
/// Define XUI_VERTEX or XUI_FRAGMENT immediately after the version line when compiling
/// a stage. Both stages use main. Bind XuiBindings as a std140 uniform block; matrices
/// are row-major in that buffer, matching Xui's CPU matrices. Vertex clip depth is
/// converted from Xui's [0,w] convention to OpenGL's [-w,w]. Texture/sampler pairs
/// become combined sampler uniforms named x_texture_TextureField_SamplerField.
/// Unsupported IR fails explicitly; this backend does not provide a GPU device.
/// </remarks>
public sealed class GlslCodeGenerator : IShaderBackend
{
    /// <summary>Gets the source-generator backend selector.</summary>
    public string Name => "GLSL";
    private readonly StringBuilder output = new();
    private readonly Dictionary<string, string> textures = new();
    private readonly Dictionary<string, IrStructType> structures = new();
    private IrStructType? bindings;
    private int indent;

    /// <summary>Generates a source bundle containing both compile-time-selected stages.</summary>
    public string GenerateCode(IrShaderModule module)
    {
        output.Clear(); textures.Clear(); structures.Clear(); indent = 0;
        foreach (var declaration in module.Structs) structures.Add(declaration.Type.Name, declaration.Type);
        if (module.Functions.Count != 0) throw new NotSupportedException("GLSL user-defined helper functions are not implemented.");
        bindings = module.VertexStage?.BindingsType ?? module.FragmentStage?.BindingsType;
        if (module.VertexStage is { BindingsType: { } vertexBindings } &&
            module.FragmentStage is { BindingsType: { } fragmentBindings } && vertexBindings != fragmentBindings)
            throw new NotSupportedException("GLSL stages require the same uniform bindings type.");
        Line("#version 300 es");
        Line("precision highp float;"); Line("precision highp int;");
        Line("#if defined(XUI_VERTEX) == defined(XUI_FRAGMENT)");
        Line("#error Define exactly one XUI shader stage"); Line("#endif");
        foreach (var declaration in module.Structs)
        {
            var fields = declaration.Type.Fields.Where(f => !Resource(f.Type)).ToArray();
            if (fields.Length == 0) continue;
            Line($"struct {Type(declaration.Type)} {{"); indent++;
            foreach (var field in fields) Line($"{Type(field.Type)} {Id(field.Name)};");
            indent--; Line("};");
        }
        if (bindings != null && bindings.Fields.Any(f => !Resource(f.Type)))
            Line($"layout(std140, row_major) uniform XuiBindings {{ {Type(bindings)} x_bindings; }};");
        // Generate the bodies first to discover exactly which texture/sampler pairs they use.
        int resourcesAt = output.Length;
        if (module.VertexStage is { } vertex)
        {
            Line("#ifdef XUI_VERTEX");
            Stage(vertex.InputType, vertex.OutputType, vertex.Body, true);
            Line("#endif");
        }
        if (module.FragmentStage is { } fragment)
        {
            Line("#ifdef XUI_FRAGMENT");
            Stage(fragment.InputType, fragment.OutputType, fragment.Body, false);
            Line("#endif");
        }
        output.Insert(resourcesAt, string.Concat(textures.Select(pair => $"uniform highp {pair.Value} {pair.Key};\n")));
        return output.ToString();
    }

    private void Stage(IrStructType input, IrStructType result, IrBlock body, bool vertex)
    {
        foreach (var field in input.Fields) Interface(field, vertex, true);
        foreach (var field in result.Fields) Interface(field, vertex, false);
        Line($"{Type(result)} x_execute({Type(input)} x_input) {{"); indent++;
        Block(body); indent--; Line("}");
        Line("void main() {"); indent++;
        Line($"{Type(input)} x_input;");
        foreach (var field in input.Fields)
            Line($"x_input.{Id(field.Name)} = {InterfaceValue(field, vertex, true)};");
        Line($"{Type(result)} x_result = x_execute(x_input);");
        foreach (var field in result.Fields)
            Line($"{InterfaceValue(field, vertex, false)} = x_result.{Id(field.Name)};");
        if (vertex)
        {
            if (!result.Fields.Any(f => f.Decorations.OfType<IrBuiltInDecoration>().Any(d => d.Semantic == BuiltInSemantic.Position)))
                throw new NotSupportedException("GLSL vertex output requires a Position field.");
            Line("gl_Position.z = 2.0 * gl_Position.z - gl_Position.w;");
        }
        indent--; Line("}");
    }

    private void Interface(IrStructField field, bool vertex, bool input)
    {
        if (field.Decorations.OfType<IrBuiltInDecoration>().Any()) return;
        var location = field.Decorations.OfType<IrLocationDecoration>().SingleOrDefault()
            ?? throw new NotSupportedException($"GLSL interface field requires a location: {field.Name}.");
        bool endpoint = vertex == input;
        string interpolation = "";
        if (!endpoint)
        {
            var mode = field.Decorations.OfType<IrInterpolationDecoration>().SingleOrDefault()?.Mode ?? InterpolationMode.Perspective;
            interpolation = mode switch {
                InterpolationMode.Flat => "flat ", InterpolationMode.Perspective => "smooth ",
                _ => throw new NotSupportedException("Non-perspective interpolation is not available in GLSL ES 3.00.")
            };
            if ((field.Type is IrScalarType { ScalarKind: ScalarKind.I32 or ScalarKind.U32 } ||
                field.Type is IrVectorType { ElementType.ScalarKind: ScalarKind.I32 or ScalarKind.U32 }) && mode != InterpolationMode.Flat)
                throw new NotSupportedException("Integer varyings require explicit flat interpolation.");
        }
        Line($"{(endpoint ? $"layout(location = {location.Location}) " : "")}{interpolation}{(input ? "in" : "out")} {Type(field.Type)} {InterfaceValue(field, vertex, input)};");
    }

    private static string InterfaceValue(IrStructField field, bool vertex, bool input)
    {
        var builtIn = field.Decorations.OfType<IrBuiltInDecoration>().SingleOrDefault();
        if (builtIn != null)
            return (vertex, input, builtIn.Semantic) switch {
                (true, false, BuiltInSemantic.Position) => "gl_Position",
                (false, true, BuiltInSemantic.Position) => "gl_FragCoord",
                (false, false, BuiltInSemantic.FragDepth) => "gl_FragDepth",
                (true, true, BuiltInSemantic.VertexId) => $"{Type(field.Type)}(gl_VertexID)",
                (true, true, BuiltInSemantic.InstanceId) => $"{Type(field.Type)}(gl_InstanceID)",
                _ => throw new NotSupportedException($"GLSL built-in is invalid for this stage: {builtIn.Semantic}.")
            };
        int location = field.Decorations.OfType<IrLocationDecoration>().Single().Location;
        return (vertex == input ? vertex ? "x_attribute_" : "x_color_" : "x_varying_") + location;
    }

    private void Block(IrBlock block)
    {
        foreach (var statement in block.Statements)
            switch (statement)
            {
                case IrVarDecl variable:
                    Line($"{Type(variable.Type)} {Id(variable.Name)} = {(variable.Initializer == null ? Zero(variable.Type) : Expression(variable.Initializer))};"); break;
                case IrAssignment assignment: Line($"{Expression(assignment.Target)} = {Expression(assignment.Value)};"); break;
                case IrReturn returned: Line(returned.Value == null ? "return;" : $"return {Expression(returned.Value)};"); break;
                case IrIf conditional:
                    Line($"if ({Expression(conditional.Condition)}) {{"); indent++; Block(conditional.ThenBlock); indent--; Line("}");
                    if (conditional.ElseBlock != null) { Line("else {"); indent++; Block(conditional.ElseBlock); indent--; Line("}"); }
                    break;
                case IrBlock child: Line("{"); indent++; Block(child); indent--; Line("}"); break;
                default: throw new NotSupportedException($"GLSL statement: {statement.GetType().Name}.");
            }
    }

    private string Expression(IrExpression expression) => expression switch {
        IrConstant constant => Constant(constant.Value),
        IrParameter parameter => Id(parameter.Name),
        IrFieldAccess field when field.Object.Type is IrVectorType => $"{Expression(field.Object)}.{field.FieldName.ToLowerInvariant()}",
        IrFieldAccess field when field.Object.Type is IrMatrixType => throw new NotSupportedException("Direct matrix row access is not implemented in GLSL."),
        IrFieldAccess field when Resource(field.Type) => throw new NotSupportedException("Shader resources must be used through a sampling intrinsic."),
        IrFieldAccess field => $"{Expression(field.Object)}.{Id(field.FieldName)}",
        IrBinaryOp binary => Binary(binary),
        IrUnaryOp unary => $"({(unary.Operator switch { UnaryOperator.Negate => "-", UnaryOperator.LogicalNot => "!", UnaryOperator.BitwiseNot => "~", _ => throw new NotSupportedException("GLSL unary operator.") })}{Expression(unary.Operand)})",
        IrConstructor constructor => Construct(constructor),
        IrMethodCall call => Call(call),
        _ => throw new NotSupportedException($"GLSL expression: {expression.GetType().Name}.")
    };

    private string Construct(IrConstructor constructor)
    {
        if (constructor.Arguments.Count == 0) return Zero(constructor.Type);
        string args = string.Join(", ", constructor.Arguments.Select(Expression));
        if (constructor.Type is not IrMatrixType matrix) return $"{Type(constructor.Type)}({args})";
        // Xui constructors take rows; GLSL constructors take columns.
        if (constructor.Arguments.Count != matrix.Rows || constructor.Arguments.Any(a =>
            a.Type is not IrVectorType { ElementType.ScalarKind: ScalarKind.F32 } row || row.Dimension != matrix.Columns))
            throw new NotSupportedException("GLSL matrix constructors require explicit float row vectors.");
        return $"transpose(mat{matrix.Rows}x{matrix.Columns}({args}))";
    }

    private string Binary(IrBinaryOp binary)
    {
        string op = binary.Operator switch {
            BinaryOperator.Add => "+", BinaryOperator.Subtract => "-", BinaryOperator.Multiply => "*", BinaryOperator.Divide => "/",
            BinaryOperator.Modulo => "%", BinaryOperator.Equal => "==", BinaryOperator.NotEqual => "!=", BinaryOperator.LessThan => "<",
            BinaryOperator.LessThanOrEqual => "<=", BinaryOperator.GreaterThan => ">", BinaryOperator.GreaterThanOrEqual => ">=",
            BinaryOperator.LogicalAnd => "&&", BinaryOperator.LogicalOr => "||", BinaryOperator.BitwiseAnd => "&", BinaryOperator.BitwiseOr => "|",
            BinaryOperator.BitwiseXor => "^", BinaryOperator.ShiftLeft => "<<", BinaryOperator.ShiftRight => ">>",
            _ => throw new NotSupportedException($"GLSL binary operator: {binary.Operator}.")
        };
        if (binary.Operator == BinaryOperator.Modulo && (binary.Left.Type is IrScalarType { ScalarKind: ScalarKind.F32 } || binary.Left.Type is IrVectorType { ElementType.ScalarKind: ScalarKind.F32 }))
            throw new NotSupportedException("Floating remainder needs truncating semantics; GLSL mod uses floor.");
        return $"({Expression(binary.Left)} {op} {Expression(binary.Right)})";
    }

    private string Call(IrMethodCall call)
    {
        if (call.Target != null) throw new NotSupportedException("GLSL instance shader methods are not implemented.");
        if (call.MethodName is "Sample" or "SampleLevel")
        {
            if (call.Arguments[0] is not IrFieldAccess texture || texture.Object.Type != bindings || texture.Type is not IrTextureType { PixelType: IrVectorType { ElementType.ScalarKind: ScalarKind.F32, Dimension: 4 } } ||
                call.Arguments[1] is not IrFieldAccess sampler || sampler.Object.Type != bindings || sampler.Type is not IrSamplerType)
                throw new NotSupportedException("GLSL sampling requires direct float4 texture and sampler bindings.");
            string name = "x_texture_" + texture.FieldName + "_" + sampler.FieldName;
            textures[name] = "sampler2D";
            return $"{(call.MethodName == "Sample" ? "texture" : "textureLod")}({name}, {string.Join(", ", call.Arguments.Skip(2).Select(Expression))})";
        }
        string args = string.Join(", ", call.Arguments.Select(Expression));
        if (call.MethodName == "Saturate") return $"clamp({args}, 0.0, 1.0)";
        string nameMapped = call.MethodName switch {
            "Clamp" => "clamp", "Lerp" => "mix", "Min" => "min", "Max" => "max", "Abs" => "abs",
            "Sin" => "sin", "Cos" => "cos", "Tan" => "tan", "Asin" => "asin", "Acos" => "acos", "Atan" or "Atan2" => "atan",
            "Pow" => "pow", "Exp" => "exp", "Log" => "log", "Log2" => "log2", "Sqrt" => "sqrt", "Rsqrt" => "inversesqrt",
            "Floor" => "floor", "Ceil" => "ceil", "Fract" => "fract", "Round" => "roundEven", "Dot" => "dot",
            "Length" => "length", "Distance" => "distance", "Normalize" => "normalize", "Cross" => "cross",
            "Ddx" => "dFdx", "Ddy" => "dFdy", "Fwidth" => "fwidth",
            _ => throw new NotSupportedException($"GLSL shader intrinsic: {call.MethodName}.")
        };
        return $"{nameMapped}({args})";
    }

    private string Zero(IrType type) => type switch {
        IrStructType structure => $"{Type(type)}({string.Join(", ", structures[structure.Name].Fields.Select(field => Zero(field.Type)))})",
        IrScalarType { ScalarKind: ScalarKind.Bool } => "false",
        IrScalarType { ScalarKind: ScalarKind.U32 } => "0u",
        IrScalarType { ScalarKind: ScalarKind.I32 } => "0",
        IrScalarType { ScalarKind: ScalarKind.F32 } => "0.0",
        IrVectorType vector => $"{Type(type)}({Zero(vector.ElementType)})",
        IrMatrixType matrix => $"{Type(type)}({Zero(matrix.ElementType)})",
        _ => throw new NotSupportedException($"GLSL zero initialization: {type.Name}.")
    };

    private static string Constant(object value) => value switch {
        float f when !float.IsNaN(f) && !float.IsInfinity(f) => Float(f),
        int i => i.ToString(CultureInfo.InvariantCulture), uint u => u.ToString(CultureInfo.InvariantCulture) + "u",
        bool b => b ? "true" : "false", _ => throw new NotSupportedException("GLSL constant type/value is not supported.")
    };
    private static string Float(float value)
    {
        string text = value.ToString("R", CultureInfo.InvariantCulture);
        return text.IndexOfAny(new[] { '.', 'E', 'e' }) < 0 ? text + ".0" : text;
    }
    private static string Type(IrType type) => type switch {
        IrScalarType scalar => scalar.ScalarKind switch { ScalarKind.F32 => "float", ScalarKind.I32 => "int", ScalarKind.U32 => "uint", ScalarKind.Bool => "bool", _ => throw new NotSupportedException("GLSL scalar type.") },
        IrVectorType vector => (vector.ElementType.ScalarKind switch { ScalarKind.F32 => "vec", ScalarKind.I32 => "ivec", ScalarKind.U32 => "uvec", ScalarKind.Bool => "bvec", _ => throw new NotSupportedException("GLSL vector type.") }) + vector.Dimension,
        IrMatrixType { ElementType.ScalarKind: ScalarKind.F32 } matrix => $"mat{matrix.Columns}x{matrix.Rows}",
        IrStructType structure => Id(structure.Name), _ => throw new NotSupportedException($"GLSL type: {type.Name}.")
    };
    private static bool Resource(IrType type) => type is IrTextureType or IrSamplerType;
    private static string Id(string name) => "x_" + name;
    private void Line(string text) => output.Append(' ', indent * 4).AppendLine(text);
}
