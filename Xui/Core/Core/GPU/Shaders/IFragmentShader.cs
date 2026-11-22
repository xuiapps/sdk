namespace Xui.GPU.Shaders;

/// <summary>
/// Fragment shader interface that computes pixel colors.
/// </summary>
/// <typeparam name="TInput">The fragment input/varyings structure type.</typeparam>
/// <typeparam name="TOutput">The fragment output structure type.</typeparam>
/// <typeparam name="TBindings">The shader resource bindings type.</typeparam>
public interface IFragmentShader<TInput, TOutput, TBindings> : IShaderStage
    where TInput : unmanaged
    where TOutput : unmanaged
    where TBindings : struct
{
    /// <summary>
    /// Executes the fragment shader for a single fragment.
    /// </summary>
    /// <param name="input">The interpolated fragment input data.</param>
    /// <param name="bindings">The shader resource bindings.</param>
    /// <returns>The computed fragment output.</returns>
    TOutput Execute(TInput input, in TBindings bindings);
}
