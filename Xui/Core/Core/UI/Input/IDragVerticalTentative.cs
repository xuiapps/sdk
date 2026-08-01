namespace Xui.Core.UI.Input;

/// <summary>
/// A vertical drag willing to yield to a horizontal ancestor until its direction
/// commits. Promote to <see cref="PointerGestures.Drag"/> once vertical intent is clear.
/// </summary>
public interface IDragVerticalTentative : IDrag
{
}
