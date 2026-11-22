namespace Xui.GPU.Shaders;

/// <summary>
/// Vertex shader interface that transforms vertex input data.
/// </summary>
/// <typeparam name="TInput">The vertex input structure type.</typeparam>
/// <typeparam name="TOutput">The vertex output/varyings structure type.</typeparam>
/// <typeparam name="TBindings">The shader resource bindings type.</typeparam>
public interface IVertexShader<TInput, TOutput, TBindings> : IShaderStage
    where TInput : unmanaged
    where TOutput : unmanaged
    where TBindings : struct
{
    /// <summary>
    /// Executes the vertex shader for a single vertex.
    /// </summary>
    /// <param name="input">The vertex input data.</param>
    /// <param name="bindings">The shader resource bindings.</param>
    /// <returns>The transformed vertex output.</returns>
    TOutput Execute(TInput input, in TBindings bindings);
}
