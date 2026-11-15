using Xui.Core.Canvas.Filter;

namespace Xui.Core.Canvas;

/// <summary>Controls source-color adjustment when selecting canvas paints.</summary>
/// <remarks>
/// Set the filter before SetFill or SetStroke. It transforms solid colors and gradient stop
/// colors as the paint is selected, including paints used for text. Existing paints and images
/// are unchanged. The filter replaces the previous adjustment and participates in Save/Restore.
/// </remarks>
public interface IFilterContext
{
    /// <summary>Disables adjustment for subsequently selected paints.</summary>
    void SetFilter();

    /// <summary>Sets the brightness of subsequently selected paints.</summary>
    void SetFilter(Brightness brightness);
}
