using System.Text;
using Xui.GPU.IR;

namespace Xui.GPU.Backends.Metal;

/// <summary>
/// Metal Shading Language (MSL) code generator for Apple platforms (macOS, iOS).
/// Translates IR to MSL shader code compatible with the Metal framework.
/// </summary>
/// <remarks>
/// MSL maps closely to C++14 with Metal-specific extensions.
/// Key differences from HLSL:
/// - Vertex/fragment functions use [[attribute(n)]] instead of semantics
/// - Built-ins use [[position]], [[color(n)]], etc.
/// - Resource bindings use [[buffer(n)]], [[texture(n)]], [[sampler(n)]]
/// - Entry points are marked with [[vertex]] and [[fragment]] attributes
/// - Structs use attribute annotations directly on fields
/// </remarks>
public class MslCodeGenerator : IShaderBackend
{
    /// <inheritdoc/>
    public string Name => "MSL";

    private readonly StringBuilder _output = new();
    private int _indentLevel = 0;
    private IrStructType? resourceBindings;
    private const string IndentString = "    ";

    /// <summary>
    /// Generates MSL shader code from an IR module.
    /// </summary>
    public string GenerateCode(IrShaderModule module)
    {
        _output.Clear();
        _indentLevel = 0;
        resourceBindings = module.VertexStage?.BindingsType ?? module.FragmentStage?.BindingsType;

        // Header comment
        WriteLine("// Generated MSL shader code");
        WriteLine($"// Module: {module.Name}");
        WriteLine();

        // MSL standard library include
        WriteLine("#include <metal_stdlib>");
        WriteLine("using namespace metal;");
        WriteLine();

        // Identify roles so struct fields get correct Metal attributes:
        //   - vertex input struct fields have no attributes (accessed via pointer+vertexId)
        //   - fragment output struct fields use [[color(N)]]
        //   - all other user-location fields use [[user(locnN)]]
        var vertexInputType = module.VertexStage?.InputType;
        var fragmentOutputType = module.FragmentStage?.OutputType;

        // Generate struct declarations
        foreach (var structDecl in module.Structs)
        {
            bool isVertexInput = structDecl.Type == vertexInputType;
            bool isFragmentOutput = structDecl.Type == fragmentOutputType;
            GenerateStructDecl(structDecl, isVertexInput, isFragmentOutput);
            WriteLine();
        }

        // Generate vertex shader if present
        if (module.VertexStage != null)
        {
            GenerateVertexStage(module.VertexStage);
            WriteLine();
        }

        // Generate fragment shader if present
        if (module.FragmentStage != null)
        {
            GenerateFragmentStage(module.FragmentStage);
            WriteLine();
        }

        return _output.ToString();
    }

    private IEnumerable<string> ResourceParameters()
    {
        int texture = 0, sampler = 0;
        if (resourceBindings is null) yield break;
        foreach (var field in resourceBindings.Fields)
        {
            if (field.Type is IrTextureType) yield return $"texture2d<float> resource_{field.Name} [[texture({texture++})]]";
            else if (field.Type is IrSamplerType) yield return $"sampler resource_{field.Name} [[sampler({sampler++})]]";
        }
    }

    private void GenerateStructDecl(IrStructDecl structDecl, bool isVertexInput = false, bool isFragmentOutput = false)
    {
        var structType = structDecl.Type;
        WriteLine($"struct {structType.Name}");
        WriteLine("{");
        _indentLevel++;

        foreach (var field in structType.Fields)
        {
            if (field.Type is IrTextureType or IrSamplerType) continue;
            var typeStr = GetMslType(field.Type);
            var attribute = GetMslAttribute(field.Decorations, isVertexInput, isFragmentOutput);
            WriteLine($"{typeStr} {field.Name}{attribute};");
        }

        _indentLevel--;
        WriteLine("};");
    }

    private void GenerateVertexStage(IrVertexStage stage)
    {
        WriteLine($"// Vertex Shader: {stage.Name}");
        var outputType = GetMslType(stage.OutputType);
        var inputType = GetMslType(stage.InputType);

        // MSL vertex functions use [[stage_in]] with a MTLVertexDescriptor to specify
        // vertex attribute formats and offsets. This lets the hardware vertex fetch unit
        // convert tightly-packed C# vertex data to GPU-native aligned types automatically.
        var paramsList = new List<string>
        {
            $"{inputType} input [[stage_in]]",
        };
        if (stage.BindingsType != null)
        {
            var bindingsType = GetMslType(stage.BindingsType);
            paramsList.Add($"constant {bindingsType}& bindings [[buffer(1)]]");
            paramsList.AddRange(ResourceParameters());
        }
        var paramsStr = string.Join(", ", paramsList);

        WriteLine($"[[vertex]] {outputType} vertex_main({paramsStr})");
        WriteLine("{");
        _indentLevel++;

        // Generate body statements
        GenerateBlock(stage.Body);

        _indentLevel--;
        WriteLine("}");
    }

    private void GenerateFragmentStage(IrFragmentStage stage)
    {
        WriteLine($"// Fragment Shader: {stage.Name}");
        var outputType = GetMslType(stage.OutputType);
        var inputType = GetMslType(stage.InputType);

        // MSL fragment functions are marked with [[fragment]]
        var extra = stage.BindingsType is null ? "" : $", constant {stage.BindingsType.Name}& bindings [[buffer(1)]]";
        var resources = ResourceParameters().ToArray();
        if (resources.Length > 0) extra += ", " + string.Join(", ", resources);
        WriteLine($"[[fragment]] {outputType} fragment_main({inputType} input [[stage_in]]{extra})");
        WriteLine("{");
        _indentLevel++;

        // Generate body statements
        GenerateBlock(stage.Body);

        _indentLevel--;
        WriteLine("}");
    }

    private void GenerateBlock(IrBlock block)
    {
        foreach (var statement in block.Statements)
        {
            GenerateStatement(statement);
        }
    }

    private void GenerateStatement(IrStatement statement)
    {
        switch (statement)
        {
            case IrVarDecl varDecl:
                GenerateVarDecl(varDecl);
                break;
            case IrAssignment assignment:
                GenerateAssignment(assignment);
                break;
            case IrReturn returnStmt:
                GenerateReturn(returnStmt);
                break;
            case IrIf ifStmt:
                GenerateIf(ifStmt);
                break;
            case IrBlock block:
                GenerateBlock(block);
                break;
            default:
                throw new InvalidOperationException(
                    $"Unsupported statement type '{statement.GetType().Name}' at {statement.SourceLocation?.ToDisplayString() ?? "unknown location"}");
        }
    }

    private void GenerateVarDecl(IrVarDecl varDecl)
    {
        var typeStr = GetMslType(varDecl.Type);
        if (varDecl.Initializer != null)
        {
            var initExpr = GenerateExpression(varDecl.Initializer);
            WriteLine($"{typeStr} {varDecl.Name} = {initExpr};");
        }
        else
        {
            WriteLine($"{typeStr} {varDecl.Name};");
        }
    }

    private void GenerateAssignment(IrAssignment assignment)
    {
        var targetExpr = GenerateExpression(assignment.Target);
        var valueExpr = GenerateExpression(assignment.Value);
        WriteLine($"{targetExpr} = {valueExpr};");
    }

    private void GenerateReturn(IrReturn returnStmt)
    {
        if (returnStmt.Value != null)
        {
            var valueExpr = GenerateExpression(returnStmt.Value);
            WriteLine($"return {valueExpr};");
        }
        else
        {
            WriteLine("return;");
        }
    }

    private void GenerateIf(IrIf ifStmt)
    {
        var condExpr = GenerateExpression(ifStmt.Condition);
        WriteLine($"if ({condExpr})");
        WriteLine("{");
        _indentLevel++;
        GenerateBlock(ifStmt.ThenBlock);
        _indentLevel--;

        if (ifStmt.ElseBlock != null)
        {
            WriteLine("}");
            WriteLine("else");
            WriteLine("{");
            _indentLevel++;
            GenerateBlock(ifStmt.ElseBlock);
            _indentLevel--;
        }

        WriteLine("}");
    }

    private string GenerateExpression(IrExpression expression)
    {
        return expression switch
        {
            IrConstant constant => GenerateConstant(constant),
            IrParameter parameter => parameter.Name,
            IrFieldAccess fieldAccess when fieldAccess.Object.Type == resourceBindings && fieldAccess.Type is IrTextureType or IrSamplerType => $"resource_{fieldAccess.FieldName}",
            IrFieldAccess fieldAccess => $"{GenerateExpression(fieldAccess.Object)}.{(fieldAccess.Object.Type is IrVectorType ? fieldAccess.FieldName.ToLowerInvariant() : fieldAccess.FieldName)}",
            IrBinaryOp binaryOp => GenerateBinaryOp(binaryOp),
            IrUnaryOp unaryOp => GenerateUnaryOp(unaryOp),
            IrMethodCall methodCall => GenerateMethodCall(methodCall),
            IrConstructor constructor => GenerateConstructor(constructor),
            _ => throw new InvalidOperationException(
                $"Unsupported expression type '{expression.GetType().Name}' at {expression.SourceLocation?.ToDisplayString() ?? "unknown location"}")
        };
    }

    private string GenerateConstant(IrConstant constant)
    {
        return constant.Value switch
        {
            float f => FloatLiteral(f),
            double d => d.ToString("R"),
            int i => i.ToString(),
            uint u => u.ToString() + "u",
            bool b => b ? "true" : "false",
            _ => throw new InvalidOperationException(
                $"Unsupported constant type '{constant.Value?.GetType().Name ?? "null"}' at {constant.SourceLocation?.ToDisplayString() ?? "unknown location"}")
        };
    }

    /// <summary>
    /// Formats a float constant for MSL. Metal requires a decimal point in float literals
    /// (e.g. <c>1.0f</c> is valid, <c>1f</c> is not).
    /// </summary>
    private static string FloatLiteral(float f)
    {
        var s = f.ToString("R");
        // Ensure the literal always contains a decimal point (Metal requirement)
        if (!s.Contains('.') && !s.Contains('e') && !s.Contains('E')
            && !s.Contains('N') && !s.Contains('n'))   // guard NaN/Infinity
        {
            s += ".0";
        }
        return s + "f";
    }

    private string GenerateBinaryOp(IrBinaryOp binaryOp)
    {
        var left = GenerateExpression(binaryOp.Left);
        var right = GenerateExpression(binaryOp.Right);

        // MSL float4x4 is column-major, but C#'s Float4x4 is row-major. When the same
        // bytes are passed via a buffer, MSL reads rows as columns (effectively transposing).
        // To get the correct result, swap matrix*vector to vector*matrix in MSL:
        //   C# row-major:    M * v  =>  result[i] = dot(row_i, v)
        //   MSL col-major:   v * M  =>  result[i] = dot(v, col_i) = dot(col_i, v) = dot(row_i, v)  ✓
        if (binaryOp.Operator == BinaryOperator.Multiply
            && binaryOp.Left.Type is IrMatrixType
            && binaryOp.Right.Type is IrVectorType)
        {
            return $"({right} * {left})";
        }

        var op = binaryOp.Operator switch
        {
            BinaryOperator.Add => "+",
            BinaryOperator.Subtract => "-",
            BinaryOperator.Multiply => "*",
            BinaryOperator.Divide => "/",
            BinaryOperator.Modulo => "%",
            BinaryOperator.Equal => "==",
            BinaryOperator.NotEqual => "!=",
            BinaryOperator.LessThan => "<",
            BinaryOperator.LessThanOrEqual => "<=",
            BinaryOperator.GreaterThan => ">",
            BinaryOperator.GreaterThanOrEqual => ">=",
            BinaryOperator.LogicalAnd => "&&",
            BinaryOperator.LogicalOr => "||",
            BinaryOperator.BitwiseAnd => "&",
            BinaryOperator.BitwiseOr => "|",
            BinaryOperator.BitwiseXor => "^",
            BinaryOperator.ShiftLeft => "<<",
            BinaryOperator.ShiftRight => ">>",
            _ => throw new InvalidOperationException(
                $"Unsupported binary operator '{binaryOp.Operator}' at {binaryOp.SourceLocation?.ToDisplayString() ?? "unknown location"}")
        };
        return $"({left} {op} {right})";
    }

    private string GenerateUnaryOp(IrUnaryOp unaryOp)
    {
        var operand = GenerateExpression(unaryOp.Operand);
        var op = unaryOp.Operator switch
        {
            UnaryOperator.Negate => "-",
            UnaryOperator.LogicalNot => "!",
            UnaryOperator.BitwiseNot => "~",
            _ => throw new InvalidOperationException(
                $"Unsupported unary operator '{unaryOp.Operator}' at {unaryOp.SourceLocation?.ToDisplayString() ?? "unknown location"}")
        };
        return $"({op}{operand})";
    }

    private string GenerateMethodCall(IrMethodCall methodCall)
    {
        if (methodCall.Target == null && methodCall.MethodName is "Sample" or "SampleLevel")
        {
            var a = methodCall.Arguments.Select(GenerateExpression).ToArray();
            return a[0] + ".sample(" + a[1] + ", " + a[2] + (a.Length == 4 ? ", level(" + a[3] + ")" : "") + ")";
        }
        var args = string.Join(", ", methodCall.Arguments.Select(GenerateExpression));

        // Map shader intrinsics to MSL built-ins
        var mslMethod = MapIntrinsicToMsl(methodCall.MethodName);

        if (methodCall.Target != null)
        {
            var target = GenerateExpression(methodCall.Target);

            // MSL texture sampling uses .sample(sampler, coords) not .Sample(sampler, coords)
            if (methodCall.MethodName == "Sample")
            {
                return $"{target}.sample({args})";
            }
            if (methodCall.MethodName == "Load")
            {
                return $"{target}.read({args})";
            }
            return $"{target}.{mslMethod}({args})";
        }

        return $"{mslMethod}({args})";
    }

    private string GenerateConstructor(IrConstructor constructor)
    {
        var typeStr = GetMslType(constructor.Type);
        var args = string.Join(", ", constructor.Arguments.Select(GenerateExpression));
        return $"{typeStr}({args})";
    }

    private string GetMslType(IrType type)
    {
        return type switch
        {
            IrScalarType scalar => scalar.ScalarKind switch
            {
                ScalarKind.F32 => "float",
                ScalarKind.I32 => "int",
                ScalarKind.U32 => "uint",
                ScalarKind.Bool => "bool",
                _ => throw new InvalidOperationException(
                    $"Unsupported scalar kind '{scalar.ScalarKind}' at {type.SourceLocation?.ToDisplayString() ?? "unknown location"}")
            },
            IrVectorType vector => GetMslVectorType(vector),
            IrMatrixType matrix => GetMslMatrixType(matrix),
            IrStructType structType => structType.Name,
            IrTextureType => "texture2d<float>",
            IrSamplerType => "sampler",
            _ => throw new InvalidOperationException(
                $"Unsupported IR type '{type.GetType().Name}' at {type.SourceLocation?.ToDisplayString() ?? "unknown location"}")
        };
    }

    private string GetMslVectorType(IrVectorType vector)
    {
        var baseType = vector.ElementType.ScalarKind switch
        {
            ScalarKind.F32 => "float",
            ScalarKind.I32 => "int",
            ScalarKind.U32 => "uint",
            ScalarKind.Bool => "bool",
            _ => throw new InvalidOperationException(
                $"Unsupported vector element type '{vector.ElementType.ScalarKind}' at {vector.SourceLocation?.ToDisplayString() ?? "unknown location"}")
        };
        return $"{baseType}{vector.Dimension}";
    }

    private string GetMslMatrixType(IrMatrixType matrix)
    {
        var baseType = matrix.ElementType.ScalarKind switch
        {
            ScalarKind.F32 => "float",
            ScalarKind.I32 => "int",
            ScalarKind.U32 => "uint",
            _ => throw new InvalidOperationException(
                $"Unsupported matrix element type '{matrix.ElementType.ScalarKind}' at {matrix.SourceLocation?.ToDisplayString() ?? "unknown location"}. Only F32, I32, and U32 are supported for matrices.")
        };
        // MSL matrix type is float4x4 (columns x rows)
        return $"{baseType}{matrix.Columns}x{matrix.Rows}";
    }

    private string GetMslAttribute(List<IrDecoration> decorations, bool isVertexInput, bool isFragmentOutput)
    {
        foreach (var decoration in decorations)
        {
            if (decoration is IrBuiltInDecoration builtIn)
            {
                return builtIn.Semantic switch
                {
                    // In MSL, position built-in is [[position]] on the output struct
                    BuiltInSemantic.Position => " [[position]]",
                    BuiltInSemantic.FragDepth => " [[depth(any)]]",
                    BuiltInSemantic.InstanceId => " [[instance_id]]",
                    BuiltInSemantic.VertexId => " [[vertex_id]]",
                    _ => throw new InvalidOperationException(
                        $"Unsupported built-in semantic '{builtIn.Semantic}' at {decoration.SourceLocation?.ToDisplayString() ?? "unknown location"}")
                };
            }
            else if (decoration is IrLocationDecoration location)
            {
                if (isVertexInput)
                    // Vertex input struct fields use [[attribute(N)]] for MTLVertexDescriptor mapping.
                    return $" [[attribute({location.Location})]]";
                else if (isFragmentOutput)
                    // Fragment output color attachments use [[color(N)]].
                    return $" [[color({location.Location})]]";
                else
                    // Inter-stage varyings (vertex output / fragment input) use [[user(locnN)]].
                    return $" [[user(locn{location.Location})]]";
            }
        }
        return "";
    }

    private string MapIntrinsicToMsl(string methodName)
    {
        // Map shader intrinsics to MSL built-ins.
        // This is a 1:1 mapping that must be kept in sync with Xui.GPU.Shaders.Intrinsics.Shader.
        // All intrinsics must have an explicit mapping - no pass-through defaults.
        return methodName switch
        {
            // Math intrinsics - identical naming between MSL and GLSL-like languages
            "Clamp" => "clamp",
            "Saturate" => "saturate",
            "Lerp" => "mix",         // Note: MSL uses 'mix' not 'lerp'
            "Min" => "min",
            "Max" => "max",
            "Abs" => "abs",

            // Trigonometric intrinsics
            "Sin" => "sin",
            "Cos" => "cos",
            "Tan" => "tan",
            "Asin" => "asin",
            "Acos" => "acos",
            "Atan" => "atan",
            "Atan2" => "atan2",

            // Exponential intrinsics
            "Pow" => "pow",
            "Exp" => "exp",
            "Log" => "log",
            "Log2" => "log2",
            "Sqrt" => "sqrt",
            "Rsqrt" => "rsqrt",

            // Rounding intrinsics
            "Floor" => "floor",
            "Ceil" => "ceil",
            "Fract" => "fract",      // Note: MSL uses 'fract' (same as GLSL), unlike HLSL's 'frac'
            "Round" => "round",

            // Vector intrinsics
            "Dot" => "dot",
            "Length" => "length",
            "Distance" => "distance",
            "Normalize" => "normalize",
            "Cross" => "cross",

            // Derivative intrinsics (fragment only)
            "Ddx" => "dfdx",         // Note: MSL uses 'dfdx' not 'ddx'
            "Ddy" => "dfdy",         // Note: MSL uses 'dfdy' not 'ddy'
            "Fwidth" => "fwidth",

            // Texture sampling - handled specially in GenerateMethodCall
            "Sample" => "sample",
            "Load" => "read",

            _ => throw new InvalidOperationException(
                $"Unsupported shader intrinsic '{methodName}'. All intrinsics must have explicit MSL mappings. " +
                $"If this is a valid intrinsic, add it to MapIntrinsicToMsl. " +
                $"Available intrinsics are defined in Xui.GPU.Shaders.Intrinsics.Shader.")
        };
    }

    private void WriteLine(string? line = null)
    {
        if (line != null)
        {
            _output.Append(new string(' ', _indentLevel * IndentString.Length));
            _output.AppendLine(line);
        }
        else
        {
            _output.AppendLine();
        }
    }
}
