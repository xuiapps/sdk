using System.Text;
using Xui.GPU.IR;

namespace Xui.GPU.Backends.Hlsl;

/// <summary>
/// HLSL code generator for DirectX shaders.
/// Translates IR to HLSL shader code.
/// </summary>
public class HlslCodeGenerator : IShaderBackend
{
    /// <summary>Gets the name of this backend ("HLSL").</summary>
    public string Name => "HLSL";
    
    private readonly StringBuilder _output = new();
    private int _indentLevel = 0;
    private IrStructType? resourceBindings;
    private const string IndentString = "    ";

    /// <summary>
    /// Generates HLSL shader code from an IR module.
    /// </summary>
    public string GenerateCode(IrShaderModule module)
    {
        _output.Clear();
        _indentLevel = 0;
        resourceBindings = module.VertexStage?.BindingsType ?? module.FragmentStage?.BindingsType;

        // Header comment
        WriteLine("// Generated HLSL shader code");
        WriteLine($"// Module: {module.Name}");
        WriteLine();

        // Fragment output fields need SV_Target semantics, not TEXCOORD
        var fragmentOutputType = module.FragmentStage?.OutputType;

        // Generate struct declarations
        foreach (var structDecl in module.Structs)
        {
            bool isFragmentOutput = structDecl.Type == fragmentOutputType;
            GenerateStructDecl(structDecl, isFragmentOutput);
            WriteLine();
        }

        // Generate constant buffer declaration if any stage uses bindings.
        // Use cbuffer (HLSL 5.0) rather than ConstantBuffer<T> (5.1+).
        var bindingsType = module.VertexStage?.BindingsType ?? module.FragmentStage?.BindingsType;
        if (bindingsType != null)
        {
            WriteLine($"cbuffer BindingsCBuffer : register(b0)");
            WriteLine("{");
            _indentLevel++;
            WriteLine($"{bindingsType.Name} bindings;");
            _indentLevel--;
            WriteLine("};");
            WriteLine();
        }

        int textureSlot = 0, samplerSlot = 0;
        if (resourceBindings != null)
            foreach (var field in resourceBindings.Fields)
            {
                if (field.Type is IrTextureType) WriteLine($"{GetHlslType(field.Type)} resource_{field.Name} : register(t{textureSlot++});");
                else if (field.Type is IrSamplerType) WriteLine($"SamplerState resource_{field.Name} : register(s{samplerSlot++});");
            }

        // Generate vertex shader if present
        if (module.VertexStage != null)
        {
            GenerateVertexStage(module.VertexStage);
            WriteLine();
        }

        // Generate fragment (pixel) shader if present
        if (module.FragmentStage != null)
        {
            GenerateFragmentStage(module.FragmentStage);
            WriteLine();
        }

        return _output.ToString();
    }

    private void GenerateStructDecl(IrStructDecl structDecl, bool isFragmentOutput = false)
    {
        var structType = structDecl.Type;
        WriteLine($"struct {structType.Name}");
        WriteLine("{");
        _indentLevel++;

        foreach (var field in structType.Fields)
        {
            if (field.Type is IrTextureType or IrSamplerType) continue;
            var typeStr = GetHlslType(field.Type);
            var interpolation = GetInterpolationModifier(field.Decorations);
            var semantic = GetSemanticString(field.Decorations, isFragmentOutput);
            WriteLine($"{interpolation}{typeStr} {field.Name}{semantic};");
        }

        _indentLevel--;
        WriteLine("};");
    }

    private void GenerateVertexStage(IrVertexStage stage)
    {
        WriteLine($"// Vertex Shader: {stage.Name}");
        var outputType = GetHlslType(stage.OutputType);
        var inputType = GetHlslType(stage.InputType);
        
        WriteLine($"{outputType} VSMain({inputType} input)");
        WriteLine("{");
        _indentLevel++;

        // Generate body statements
        GenerateBlock(stage.Body);

        _indentLevel--;
        WriteLine("}");
    }

    private void GenerateFragmentStage(IrFragmentStage stage)
    {
        WriteLine($"// Pixel Shader: {stage.Name}");
        var outputType = GetHlslType(stage.OutputType);
        var inputType = GetHlslType(stage.InputType);
        
        WriteLine($"{outputType} PSMain({inputType} input)");
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
                WriteLine($"// TODO: Unsupported statement type {statement.GetType().Name}");
                break;
        }
    }

    private void GenerateVarDecl(IrVarDecl varDecl)
    {
        var typeStr = GetHlslType(varDecl.Type);
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
            double d => d.ToString("R"),       // Round-trip format preserves exact value
            int i => i.ToString(),
            uint u => u.ToString() + "u",
            bool b => b ? "true" : "false",
            _ => throw new InvalidOperationException(
                $"Unsupported constant type '{constant.Value?.GetType().Name ?? "null"}' at {constant.SourceLocation?.ToDisplayString() ?? "unknown location"}")
        };
    }

    private static string FloatLiteral(float f)
    {
        var s = f.ToString("R");
        if (!s.Contains('.') && !s.Contains('e') && !s.Contains('E')
            && !s.Contains('N') && !s.Contains('n'))
        {
            s += ".0";
        }
        return s + "f";
    }

    private string GenerateBinaryOp(IrBinaryOp binaryOp)
    {
        var left = GenerateExpression(binaryOp.Left);
        var right = GenerateExpression(binaryOp.Right);

        // In HLSL, * between a matrix and a vector/matrix is component-wise, not linear algebra.
        // Matrix-vector and matrix-matrix multiplication requires mul().
        if (binaryOp.Operator == BinaryOperator.Multiply &&
            (binaryOp.Left.Type is IrMatrixType || binaryOp.Right.Type is IrMatrixType))
        {
            return $"mul({left}, {right})";
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
            return a[0] + "." + methodCall.MethodName + "(" + string.Join(", ", a.Skip(1)) + ")";
        }
        var args = string.Join(", ", methodCall.Arguments.Select(GenerateExpression));
        
        // Map shader intrinsics to HLSL built-ins
        var hlslMethod = MapIntrinsicToHlsl(methodCall.MethodName);
        
        if (methodCall.Target != null)
        {
            var target = GenerateExpression(methodCall.Target);
            return $"{target}.{hlslMethod}({args})";
        }
        
        return $"{hlslMethod}({args})";
    }

    private string GenerateConstructor(IrConstructor constructor)
    {
        var typeStr = GetHlslType(constructor.Type);
        var args = string.Join(", ", constructor.Arguments.Select(GenerateExpression));
        return $"{typeStr}({args})";
    }

    private string GetHlslType(IrType type)
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
            IrVectorType vector => GetHlslVectorType(vector),
            IrMatrixType matrix => GetHlslMatrixType(matrix),
            IrStructType structType => structType.Name,
            IrTextureType texture => $"Texture2D<{GetHlslType(texture.PixelType)}>",
            IrSamplerType => "SamplerState",
            _ => throw new InvalidOperationException(
                $"Unsupported IR type '{type.GetType().Name}' at {type.SourceLocation?.ToDisplayString() ?? "unknown location"}")
        };
    }

    private string GetHlslVectorType(IrVectorType vector)
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

    private string GetHlslMatrixType(IrMatrixType matrix)
    {
        var baseType = matrix.ElementType.ScalarKind switch
        {
            ScalarKind.F32 => "float",
            ScalarKind.I32 => "int",
            ScalarKind.U32 => "uint",
            _ => throw new InvalidOperationException(
                $"Unsupported matrix element type '{matrix.ElementType.ScalarKind}' at {matrix.SourceLocation?.ToDisplayString() ?? "unknown location"}. Only F32, I32, and U32 are supported for matrices.")
        };
        return $"{baseType}{matrix.Rows}x{matrix.Columns}";
    }

    private string GetSemanticString(List<IrDecoration> decorations, bool isFragmentOutput = false)
    {
        foreach (var decoration in decorations)
        {
            if (decoration is IrBuiltInDecoration builtIn)
            {
                return builtIn.Semantic switch
                {
                    BuiltInSemantic.Position => " : SV_POSITION",
                    BuiltInSemantic.FragDepth => " : SV_DEPTH",
                    BuiltInSemantic.InstanceId => " : SV_InstanceID",
                    BuiltInSemantic.VertexId => " : SV_VertexID",
                    _ => throw new InvalidOperationException(
                        $"Unsupported built-in semantic '{builtIn.Semantic}' at {decoration.SourceLocation?.ToDisplayString() ?? "unknown location"}")
                };
            }
            else if (decoration is IrLocationDecoration location)
            {
                return isFragmentOutput
                    ? $" : SV_Target{location.Location}"
                    : $" : TEXCOORD{location.Location}";
            }
        }
        return "";
    }

    private string GetInterpolationModifier(List<IrDecoration> decorations)
    {
        foreach (var decoration in decorations)
        {
            if (decoration is IrInterpolationDecoration interpolation)
            {
                return interpolation.Mode switch
                {
                    InterpolationMode.Flat => "nointerpolation ",
                    InterpolationMode.Linear => "noperspective ",
                    InterpolationMode.Perspective => "",  // Default in HLSL
                    _ => throw new InvalidOperationException(
                        $"Unsupported interpolation mode '{interpolation.Mode}' at {decoration.SourceLocation?.ToDisplayString() ?? "unknown location"}")
                };
            }
        }
        return "";  // Default is perspective-correct
    }

    private string MapIntrinsicToHlsl(string methodName)
    {
        // Map shader intrinsics to HLSL built-ins
        // This is a 1:1 mapping that must be kept in sync with Xui.GPU.Shaders.Intrinsics.Shader
        return methodName switch
        {
            "Clamp" => "clamp",
            "Saturate" => "saturate",
            "Lerp" => "lerp",
            "Min" => "min",
            "Max" => "max",
            "Abs" => "abs",
            "Sin" => "sin",
            "Cos" => "cos",
            "Tan" => "tan",
            "Asin" => "asin",
            "Acos" => "acos",
            "Atan" => "atan",
            "Atan2" => "atan2",
            "Pow" => "pow",
            "Exp" => "exp",
            "Log" => "log",
            "Log2" => "log2",
            "Sqrt" => "sqrt",
            "Rsqrt" => "rsqrt",
            "Floor" => "floor",
            "Ceil" => "ceil",
            "Fract" => "frac",  // Note: HLSL uses 'frac' not 'fract'
            "Round" => "round",
            "Dot" => "dot",
            "Length" => "length",
            "Distance" => "distance",
            "Normalize" => "normalize",
            "Cross" => "cross",
            "Ddx" => "ddx",
            "Ddy" => "ddy",
            "Fwidth" => "fwidth",
            "Sample" => "Sample",
            "Load" => "Load",
            _ => throw new InvalidOperationException(
                $"Unsupported shader intrinsic '{methodName}'. All intrinsics must have explicit HLSL mappings. " +
                $"If this is a valid intrinsic, add it to MapIntrinsicToHlsl. " +
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
