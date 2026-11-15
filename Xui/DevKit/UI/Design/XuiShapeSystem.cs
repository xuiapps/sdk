using Xui.Core.Canvas;

namespace Xui.DevKit.UI.Design;

/// <summary>
/// Concrete shape system that maps corner-radius tokens from a <see cref="ShapePreset"/>.
/// </summary>
internal class XuiShapeSystem : IShapeSystem
{
    public XuiShapeSystem(XuiDesignSystemOptions options)
    {
        RoundnessFactor = options.RoundnessFactor;
        CornerRadius Radius(nfloat value) => new(value * RoundnessFactor);
        var preset = options.Shape;

        switch (preset)
        {
            case ShapePreset.Square:
                None       = new CornerRadius(0);
                ExtraSmall = new CornerRadius(0);
                Small      = new CornerRadius(0);
                Medium     = new CornerRadius(0);
                Large      = new CornerRadius(0);
                ExtraLarge = new CornerRadius(0);
                Full       = new CornerRadius(0);
                break;

            case ShapePreset.Desktop:
                None       = new CornerRadius(0);
                ExtraSmall = new CornerRadius(0);
                Small      = new CornerRadius(0);
                Medium     = Radius(3);
                Large      = Radius(7);
                ExtraLarge = Radius(10);
                Full       = new CornerRadius(0);  // square buttons on desktop
                break;

            case ShapePreset.Rounded:
                None       = new CornerRadius(0);
                ExtraSmall = Radius(2);
                Small      = Radius(4);
                Medium     = Radius(8);
                Large      = Radius(14);
                ExtraLarge = Radius(20);
                Full       = Radius(4);   // small button radius
                break;

            case ShapePreset.RoundedPill:
                None       = new CornerRadius(0);
                ExtraSmall = Radius(2);
                Small      = Radius(4);
                Medium     = Radius(8);
                Large      = Radius(14);
                ExtraLarge = Radius(20);
                Full       = new CornerRadius(9999); // pill buttons
                break;

            case ShapePreset.Soft:
            default:
                None       = new CornerRadius(0);
                ExtraSmall = Radius(4);
                Small      = Radius(8);
                Medium     = Radius(12);
                Large      = Radius(20);
                ExtraLarge = Radius(28);
                Full       = new CornerRadius(9999); // pill buttons
                break;
        }
    }

    /// <inheritdoc/>
    public nfloat RoundnessFactor { get; }

    /// <inheritdoc/>
    public CornerRadius None { get; }

    /// <inheritdoc/>
    public CornerRadius ExtraSmall { get; }

    /// <inheritdoc/>
    public CornerRadius Small { get; }

    /// <inheritdoc/>
    public CornerRadius Medium { get; }

    /// <inheritdoc/>
    public CornerRadius Large { get; }

    /// <inheritdoc/>
    public CornerRadius ExtraLarge { get; }

    /// <inheritdoc/>
    public CornerRadius Full { get; }
}
