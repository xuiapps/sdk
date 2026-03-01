using System.Runtime.CompilerServices;

namespace Xui.Core.UI.Layers;

/// <summary>
/// Inline buffer holding up to 4 layer instances of the same type.
/// Pass as the <c>TBuffer</c> parameter to <see cref="HorizontalMonoStack{TChild,TBuffer}"/>,
/// <see cref="VerticalMonoStack{TChild,TBuffer}"/>, or <see cref="UniformMonoGrid{TChild,TBuffer}"/>.
/// </summary>
[InlineArray(4)]
public struct LayerBuffer4<T> { private T _element0; }

/// <summary>Inline buffer holding up to 8 layer instances of the same type.</summary>
[InlineArray(8)]
public struct LayerBuffer8<T> { private T _element0; }

/// <summary>Inline buffer holding up to 16 layer instances of the same type.</summary>
[InlineArray(16)]
public struct LayerBuffer16<T> { private T _element0; }

/// <summary>Inline buffer holding up to 32 layer instances of the same type.</summary>
[InlineArray(32)]
public struct LayerBuffer32<T> { private T _element0; }
