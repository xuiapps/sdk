using System.Runtime.CompilerServices;

namespace Xui.Core.UI.Input;

/// <summary>A handler advertising a possible gesture during pointer-down tunneling.</summary>
/// <param name="Handler">The view which may later claim capture.</param>
/// <param name="Gesture">The gesture it may recognize.</param>
public readonly record struct PointerGestureCandidate(View Handler, IPointerGesture Gesture);

// A local instance is stack storage containing eight GC-tracked pairs of references.
// Unlike stackalloc, InlineArray also supports elements containing managed references.
[InlineArray(8)]
internal struct PointerGestureBuffer
{
    private PointerGestureCandidate element0;
}
