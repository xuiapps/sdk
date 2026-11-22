using Xui.GPU.Shaders.Types;

namespace Xui.GPU.Shaders.Intrinsics;

/// <summary>
/// Provides shader intrinsic functions for mathematical operations.
/// These functions mirror GPU shader intrinsics and will be translated
/// to appropriate backend-specific implementations.
/// </summary>
public static class Shader
{
    // ===== Basic Math Intrinsics =====

    /// <summary>
    /// Clamps a value between a minimum and maximum value.
    /// </summary>
    public static F32 Clamp(F32 value, F32 min, F32 max)
    {
        float v = value;
        float minVal = min;
        float maxVal = max;
        if (v < minVal) return min;
        if (v > maxVal) return max;
        return value;
    }

    /// <summary>
    /// Saturates a value to the range [0, 1].
    /// </summary>
    public static F32 Saturate(F32 value)
    {
        return Clamp(value, F32.Zero, F32.One);
    }

    /// <summary>
    /// Linearly interpolates between two values.
    /// </summary>
    /// <param name="a">The start value.</param>
    /// <param name="b">The end value.</param>
    /// <param name="t">The interpolation factor (typically 0-1).</param>
    public static F32 Lerp(F32 a, F32 b, F32 t)
    {
        float av = a;
        float bv = b;
        float tv = t;
        return new F32(av + (bv - av) * tv);
    }

    /// <summary>
    /// Returns the minimum of two values.
    /// </summary>
    public static F32 Min(F32 a, F32 b)
    {
        float av = a;
        float bv = b;
        return av < bv ? a : b;
    }

    /// <summary>
    /// Returns the maximum of two values.
    /// </summary>
    public static F32 Max(F32 a, F32 b)
    {
        float av = a;
        float bv = b;
        return av > bv ? a : b;
    }

    /// <summary>
    /// Returns the absolute value.
    /// </summary>
    public static F32 Abs(F32 value)
    {
        float v = value;
        return new F32(MathF.Abs(v));
    }

    // ===== Trigonometric Intrinsics =====

    /// <summary>
    /// Returns the sine of the specified angle in radians.
    /// </summary>
    public static F32 Sin(F32 angle)
    {
        float a = angle;
        return new F32(MathF.Sin(a));
    }

    /// <summary>
    /// Returns the cosine of the specified angle in radians.
    /// </summary>
    public static F32 Cos(F32 angle)
    {
        float a = angle;
        return new F32(MathF.Cos(a));
    }

    /// <summary>
    /// Returns the tangent of the specified angle in radians.
    /// </summary>
    public static F32 Tan(F32 angle)
    {
        float a = angle;
        return new F32(MathF.Tan(a));
    }

    /// <summary>
    /// Returns the arc sine (inverse sine) in radians.
    /// </summary>
    public static F32 Asin(F32 value)
    {
        float v = value;
        return new F32(MathF.Asin(v));
    }

    /// <summary>
    /// Returns the arc cosine (inverse cosine) in radians.
    /// </summary>
    public static F32 Acos(F32 value)
    {
        float v = value;
        return new F32(MathF.Acos(v));
    }

    /// <summary>
    /// Returns the arc tangent (inverse tangent) in radians.
    /// </summary>
    public static F32 Atan(F32 value)
    {
        float v = value;
        return new F32(MathF.Atan(v));
    }

    /// <summary>
    /// Returns the arc tangent of y/x in radians.
    /// </summary>
    public static F32 Atan2(F32 y, F32 x)
    {
        float yv = y;
        float xv = x;
        return new F32(MathF.Atan2(yv, xv));
    }

    // ===== Power and Exponential Intrinsics =====

    /// <summary>
    /// Returns the value raised to the specified power.
    /// </summary>
    public static F32 Pow(F32 value, F32 exponent)
    {
        float v = value;
        float e = exponent;
        return new F32(MathF.Pow(v, e));
    }

    /// <summary>
    /// Returns the exponential (e^x).
    /// </summary>
    public static F32 Exp(F32 value)
    {
        float v = value;
        return new F32(MathF.Exp(v));
    }

    /// <summary>
    /// Returns the natural logarithm.
    /// </summary>
    public static F32 Log(F32 value)
    {
        float v = value;
        return new F32(MathF.Log(v));
    }

    /// <summary>
    /// Returns the base-2 logarithm.
    /// </summary>
    public static F32 Log2(F32 value)
    {
        float v = value;
        return new F32(MathF.Log2(v));
    }

    /// <summary>
    /// Returns the square root.
    /// </summary>
    public static F32 Sqrt(F32 value)
    {
        float v = value;
        return new F32(MathF.Sqrt(v));
    }

    /// <summary>
    /// Returns the reciprocal square root (1 / sqrt(x)).
    /// </summary>
    public static F32 Rsqrt(F32 value)
    {
        float v = value;
        return new F32(1.0f / MathF.Sqrt(v));
    }

    // ===== Rounding and Floor/Ceil Intrinsics =====

    /// <summary>
    /// Returns the largest integer less than or equal to the value.
    /// </summary>
    public static F32 Floor(F32 value)
    {
        float v = value;
        return new F32(MathF.Floor(v));
    }

    /// <summary>
    /// Returns the smallest integer greater than or equal to the value.
    /// </summary>
    public static F32 Ceil(F32 value)
    {
        float v = value;
        return new F32(MathF.Ceiling(v));
    }

    /// <summary>
    /// Returns the fractional part of the value.
    /// </summary>
    public static F32 Fract(F32 value)
    {
        float v = value;
        return new F32(v - MathF.Floor(v));
    }

    /// <summary>
    /// Returns the value rounded to the nearest integer.
    /// </summary>
    public static F32 Round(F32 value)
    {
        float v = value;
        return new F32(MathF.Round(v));
    }

    // ===== Vector Intrinsics =====

    /// <summary>
    /// Returns the dot product of two 2D vectors.
    /// </summary>
    public static F32 Dot(Float2 a, Float2 b)
    {
        return a.X * b.X + a.Y * b.Y;
    }

    /// <summary>
    /// Returns the dot product of two 3D vectors.
    /// </summary>
    public static F32 Dot(Float3 a, Float3 b)
    {
        return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
    }

    /// <summary>
    /// Returns the dot product of two 4D vectors.
    /// </summary>
    public static F32 Dot(Float4 a, Float4 b)
    {
        return a.X * b.X + a.Y * b.Y + a.Z * b.Z + a.W * b.W;
    }

    /// <summary>
    /// Returns the length (magnitude) of a 2D vector.
    /// </summary>
    public static F32 Length(Float2 v)
    {
        return Sqrt(Dot(v, v));
    }

    /// <summary>
    /// Returns the length (magnitude) of a 3D vector.
    /// </summary>
    public static F32 Length(Float3 v)
    {
        return Sqrt(Dot(v, v));
    }

    /// <summary>
    /// Returns the length (magnitude) of a 4D vector.
    /// </summary>
    public static F32 Length(Float4 v)
    {
        return Sqrt(Dot(v, v));
    }

    /// <summary>
    /// Returns the distance between two 2D points.
    /// </summary>
    public static F32 Distance(Float2 a, Float2 b)
    {
        return Length(a - b);
    }

    /// <summary>
    /// Returns the distance between two 3D points.
    /// </summary>
    public static F32 Distance(Float3 a, Float3 b)
    {
        return Length(a - b);
    }

    /// <summary>
    /// Returns the distance between two 4D points.
    /// </summary>
    public static F32 Distance(Float4 a, Float4 b)
    {
        return Length(a - b);
    }

    /// <summary>
    /// Returns the normalized 2D vector (length = 1).
    /// </summary>
    public static Float2 Normalize(Float2 v)
    {
        var len = Length(v);
        return v / len;
    }

    /// <summary>
    /// Returns the normalized 3D vector (length = 1).
    /// </summary>
    public static Float3 Normalize(Float3 v)
    {
        var len = Length(v);
        return v / len;
    }

    /// <summary>
    /// Returns the normalized 4D vector (length = 1).
    /// </summary>
    public static Float4 Normalize(Float4 v)
    {
        var len = Length(v);
        return v / len;
    }

    /// <summary>
    /// Returns the cross product of two 3D vectors.
    /// </summary>
    public static Float3 Cross(Float3 a, Float3 b)
    {
        return new Float3(
            a.Y * b.Z - a.Z * b.Y,
            a.Z * b.X - a.X * b.Z,
            a.X * b.Y - a.Y * b.X
        );
    }

    /// <summary>
    /// Linearly interpolates between two 2D vectors.
    /// </summary>
    public static Float2 Lerp(Float2 a, Float2 b, F32 t)
    {
        return a + (b - a) * t;
    }

    /// <summary>
    /// Linearly interpolates between two 3D vectors.
    /// </summary>
    public static Float3 Lerp(Float3 a, Float3 b, F32 t)
    {
        return a + (b - a) * t;
    }

    /// <summary>
    /// Linearly interpolates between two 4D vectors.
    /// </summary>
    public static Float4 Lerp(Float4 a, Float4 b, F32 t)
    {
        return a + (b - a) * t;
    }

    /// <summary>
    /// Component-wise minimum of two 2D vectors.
    /// </summary>
    public static Float2 Min(Float2 a, Float2 b)
    {
        return new Float2(Min(a.X, b.X), Min(a.Y, b.Y));
    }

    /// <summary>
    /// Component-wise minimum of two 3D vectors.
    /// </summary>
    public static Float3 Min(Float3 a, Float3 b)
    {
        return new Float3(Min(a.X, b.X), Min(a.Y, b.Y), Min(a.Z, b.Z));
    }

    /// <summary>
    /// Component-wise minimum of two 4D vectors.
    /// </summary>
    public static Float4 Min(Float4 a, Float4 b)
    {
        return new Float4(Min(a.X, b.X), Min(a.Y, b.Y), Min(a.Z, b.Z), Min(a.W, b.W));
    }

    /// <summary>
    /// Component-wise maximum of two 2D vectors.
    /// </summary>
    public static Float2 Max(Float2 a, Float2 b)
    {
        return new Float2(Max(a.X, b.X), Max(a.Y, b.Y));
    }

    /// <summary>
    /// Component-wise maximum of two 3D vectors.
    /// </summary>
    public static Float3 Max(Float3 a, Float3 b)
    {
        return new Float3(Max(a.X, b.X), Max(a.Y, b.Y), Max(a.Z, b.Z));
    }

    /// <summary>
    /// Component-wise maximum of two 4D vectors.
    /// </summary>
    public static Float4 Max(Float4 a, Float4 b)
    {
        return new Float4(Max(a.X, b.X), Max(a.Y, b.Y), Max(a.Z, b.Z), Max(a.W, b.W));
    }

    /// <summary>
    /// Component-wise clamp of a 2D vector.
    /// </summary>
    public static Float2 Clamp(Float2 value, Float2 min, Float2 max)
    {
        return new Float2(Clamp(value.X, min.X, max.X), Clamp(value.Y, min.Y, max.Y));
    }

    /// <summary>
    /// Component-wise clamp of a 3D vector.
    /// </summary>
    public static Float3 Clamp(Float3 value, Float3 min, Float3 max)
    {
        return new Float3(
            Clamp(value.X, min.X, max.X),
            Clamp(value.Y, min.Y, max.Y),
            Clamp(value.Z, min.Z, max.Z)
        );
    }

    /// <summary>
    /// Component-wise clamp of a 4D vector.
    /// </summary>
    public static Float4 Clamp(Float4 value, Float4 min, Float4 max)
    {
        return new Float4(
            Clamp(value.X, min.X, max.X),
            Clamp(value.Y, min.Y, max.Y),
            Clamp(value.Z, min.Z, max.Z),
            Clamp(value.W, min.W, max.W)
        );
    }

    /// <summary>
    /// Saturates a 2D vector to the range [0, 1] component-wise.
    /// </summary>
    public static Float2 Saturate(Float2 value)
    {
        return new Float2(Saturate(value.X), Saturate(value.Y));
    }

    /// <summary>
    /// Saturates a 3D vector to the range [0, 1] component-wise.
    /// </summary>
    public static Float3 Saturate(Float3 value)
    {
        return new Float3(Saturate(value.X), Saturate(value.Y), Saturate(value.Z));
    }

    /// <summary>
    /// Saturates a 4D vector to the range [0, 1] component-wise.
    /// </summary>
    public static Float4 Saturate(Float4 value)
    {
        return new Float4(Saturate(value.X), Saturate(value.Y), Saturate(value.Z), Saturate(value.W));
    }

    // ===== Fragment Shader Derivative Intrinsics =====
    // Note: These functions only work in fragment shaders and require
    // the software renderer to execute fragments in 2x2 quads.

    /// <summary>
    /// Returns the partial derivative of the value with respect to screen-space X.
    /// Only available in fragment shaders.
    /// </summary>
    /// <remarks>
    /// This intrinsic requires fragment execution in 2x2 quads to compute derivatives.
    /// In the software renderer, this will be implemented by comparing adjacent fragment values.
    /// </remarks>
    public static F32 Ddx(F32 value)
    {
        // This is a placeholder implementation.
        // In actual shader execution, this will be computed by the fragment processor
        // by comparing values across a 2x2 fragment quad.
        throw new NotImplementedException(
            "Ddx can only be called within fragment shader execution context. " +
            "This intrinsic will be implemented by the software renderer's fragment processor."
        );
    }

    /// <summary>
    /// Returns the partial derivative of the value with respect to screen-space Y.
    /// Only available in fragment shaders.
    /// </summary>
    /// <remarks>
    /// This intrinsic requires fragment execution in 2x2 quads to compute derivatives.
    /// In the software renderer, this will be implemented by comparing adjacent fragment values.
    /// </remarks>
    public static F32 Ddy(F32 value)
    {
        // This is a placeholder implementation.
        // In actual shader execution, this will be computed by the fragment processor
        // by comparing values across a 2x2 fragment quad.
        throw new NotImplementedException(
            "Ddy can only be called within fragment shader execution context. " +
            "This intrinsic will be implemented by the software renderer's fragment processor."
        );
    }

    /// <summary>
    /// Returns the sum of absolute values of ddx and ddy.
    /// Only available in fragment shaders.
    /// </summary>
    public static F32 Fwidth(F32 value)
    {
        return Abs(Ddx(value)) + Abs(Ddy(value));
    }

    // ===== Texture Sampling Intrinsics =====
    // Note: These functions will be implemented by the software renderer
    // and translated to appropriate backend-specific texture sampling operations.

    /// <summary>
    /// Samples a 2D texture at the specified UV coordinates using a sampler.
    /// </summary>
    /// <param name="texture">The texture to sample from.</param>
    /// <param name="sampler">The sampler defining filtering and addressing modes.</param>
    /// <param name="uv">The UV texture coordinates.</param>
    /// <returns>The sampled color value.</returns>
    /// <remarks>
    /// This is a placeholder implementation. Actual texture sampling will be
    /// implemented by the software renderer or hardware backend.
    /// </remarks>
    public static Color4 Sample(Texture2D<Color4> texture, Sampler sampler, Float2 uv)
    {
        if (texture.Data is ISampledTexture<Color4> sampled) return sampled.Sample(sampler, uv, F32.Zero);
        throw new NotImplementedException(
            "Texture sampling can only be performed within shader execution context. " +
            "This intrinsic will be implemented by the renderer backend."
        );
    }

    /// <summary>Samples a texture at an explicit mip level, interpolating adjacent levels.</summary>
    public static Color4 SampleLevel(Texture2D<Color4> texture, Sampler sampler, Float2 uv, F32 level) =>
        texture.Data is ISampledTexture<Color4> sampled ? sampled.Sample(sampler, uv, level)
            : throw new NotSupportedException("The texture has no CPU sampling implementation.");

    /// <summary>
    /// Loads a texel from a 2D texture at the specified integer coordinates.
    /// </summary>
    /// <param name="texture">The texture to load from.</param>
    /// <param name="location">The integer texel coordinates.</param>
    /// <returns>The texel color value.</returns>
    /// <remarks>
    /// Unlike Sample(), Load() does not perform filtering and uses integer coordinates.
    /// This is a placeholder implementation that will be implemented by the renderer backend.
    /// </remarks>
    public static Color4 Load(Texture2D<Color4> texture, Int2 location)
    {
        throw new NotImplementedException(
            "Texture loading can only be performed within shader execution context. " +
            "This intrinsic will be implemented by the renderer backend."
        );
    }
}
