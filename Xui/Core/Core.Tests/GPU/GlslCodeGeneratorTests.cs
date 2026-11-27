using System.Globalization;
using Xui.GPU.Backends.Glsl;
using Xui.GPU.Backends.Hlsl;
using Xunit;

namespace Xui.GPU.IR.Tests;

public class GlslCodeGeneratorTests
{
    [Fact]
    public void LiteralsAreInvariantAndIdentifiersAvoidReservedWords()
    {
        var saved = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
            var module = IrBuilder.CreateTriangleShaderModule();
            var scalar = new IrScalarType(ScalarKind.F32);
            module.VertexStage!.Body.Statements.Insert(0, new IrVarDecl("uniform", scalar, new IrConstant(scalar, 0.25f)));
            string source = new GlslCodeGenerator().GenerateCode(module);
            Assert.Contains("float x_uniform = 0.25;", source);
            Assert.Contains("x_TriangleVaryings x_output = x_TriangleVaryings(vec4(0.0), vec4(0.0));", source);
            Assert.Contains("x_TriangleVaryings x_execute(x_TriangleVertex x_input)", source);
            Assert.Contains("gl_Position.z = 2.0 * gl_Position.z - gl_Position.w;", source);
        }
        finally { CultureInfo.CurrentCulture = saved; }
    }

    [Fact]
    public void MatrixConstructorsTransposeCpuRowsIntoGlslColumns()
    {
        var module = IrBuilder.CreateTriangleShaderModule();
        var scalar = new IrScalarType(ScalarKind.F32);
        var row = new IrVectorType(scalar, 3);
        var matrix = new IrMatrixType(scalar, 2, 3);
        module.VertexStage!.Body.Statements.Insert(0, new IrVarDecl("matrix", matrix,
            new IrConstructor(matrix, [new IrConstructor(row, [new IrConstant(scalar, 1f)]), new IrConstructor(row, [new IrConstant(scalar, 2f)])])));
        string source = new GlslCodeGenerator().GenerateCode(module);
        Assert.Contains("mat3x2 x_matrix = transpose(mat2x3(vec3(1.0), vec3(2.0)));", source);
    }

    [Fact]
    public void UnsupportedInterpolationFailsInsteadOfChangingSemantics()
    {
        var module = IrBuilder.CreateTriangleShaderModule();
        module.VertexStage!.OutputType.Fields[1].Decorations.Add(new IrInterpolationDecoration(InterpolationMode.Linear));
        Assert.Contains("Non-perspective interpolation", Assert.Throws<NotSupportedException>(() => new GlslCodeGenerator().GenerateCode(module)).Message);
    }

    [Fact]
    public void FloatingRemainderDoesNotUseFloorBasedMod()
    {
        var module = IrBuilder.CreateTriangleShaderModule();
        var scalar = new IrScalarType(ScalarKind.F32);
        module.VertexStage!.Body.Statements.Insert(0, new IrVarDecl("remainder", scalar,
            new IrBinaryOp(BinaryOperator.Modulo, new IrConstant(scalar, -3f), new IrConstant(scalar, 2f), scalar)));
        Assert.Contains("Floating remainder", Assert.Throws<NotSupportedException>(() => new GlslCodeGenerator().GenerateCode(module)).Message);
    }

    [Fact]
    public void HelperFunctionsAreNotSilentlyDropped()
    {
        var module = IrBuilder.CreateTriangleShaderModule();
        module.Functions.Add(new IrFunctionDecl { Name = "Helper" });
        Assert.Contains("helper functions", Assert.Throws<NotSupportedException>(() => new GlslCodeGenerator().GenerateCode(module)).Message);
    }
}
