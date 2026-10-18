namespace Xui.Core.UI.Layout;

/// <summary>
/// A container view that arranges children along a main axis with flexible sizing and wrapping.
/// Container-level properties mirror the CSS Flex Box spec.
/// </summary>
/// <remarks>
/// Supports row/column layout, wrapping, order, gaps, flexible sizing, and space distribution.
/// Item baseline alignment currently falls back to cross-axis start; line baseline alignment
/// falls back to start. Views do not yet expose a common layout-baseline contract.
/// </remarks>
public partial class FlexBox : ViewCollection
{
    private Direction flexDirection = Direction.Row;

    /// <summary>
    /// Sets the main axis direction along which flex items are placed.
    /// Corresponds to <c>flex-direction</c>. Default is <see cref="Direction.Row"/>.
    /// </summary>
    public Direction FlexDirection
    {
        get => flexDirection;
        set => SetViewProperty(ref flexDirection, value);
    }

    private Wrap flexWrap = Wrap.NoWrap;

    /// <summary>
    /// Controls whether flex items wrap onto multiple lines.
    /// Corresponds to <c>flex-wrap</c>. Default is <see cref="Wrap.NoWrap"/>.
    /// </summary>
    public Wrap FlexWrap
    {
        get => flexWrap;
        set => SetViewProperty(ref flexWrap, value);
    }

    private JustifyContent flexJustifyContent = JustifyContent.FlexStart;

    /// <summary>
    /// Aligns flex items along the main axis when there is free space.
    /// Corresponds to <c>justify-content</c>. Default is <see cref="JustifyContent.FlexStart"/>.
    /// </summary>
    public JustifyContent FlexJustifyContent
    {
        get => flexJustifyContent;
        set => SetViewProperty(ref flexJustifyContent, value);
    }

    private AlignItems flexAlignItems = AlignItems.Stretch;

    /// <summary>
    /// Sets the default cross-axis alignment for all flex items on a line.
    /// Corresponds to <c>align-items</c>. Default is <see cref="AlignItems.Stretch"/>.
    /// </summary>
    public AlignItems FlexAlignItems
    {
        get => flexAlignItems;
        set => SetViewProperty(ref flexAlignItems, value);
    }

    private AlignContent flexAlignContent = AlignContent.Normal;

    /// <summary>
    /// Aligns flex lines along the cross axis when there is extra space (only applies when wrapping).
    /// Corresponds to <c>align-content</c>. Default is <see cref="AlignContent.Normal"/>.
    /// </summary>
    public AlignContent FlexAlignContent
    {
        get => flexAlignContent;
        set => SetViewProperty(ref flexAlignContent, value);
    }

    private nfloat rowGap = 0;

    /// <summary>
    /// Space between rows of flex items.
    /// Corresponds to <c>row-gap</c>. Default is <c>0</c>.
    /// </summary>
    public nfloat RowGap
    {
        get => rowGap;
        set => SetViewProperty(ref rowGap, value);
    }

    private nfloat columnGap = 0;

    /// <summary>
    /// Space between columns of flex items.
    /// Corresponds to <c>column-gap</c>. Default is <c>0</c>.
    /// </summary>
    public nfloat ColumnGap
    {
        get => columnGap;
        set => SetViewProperty(ref columnGap, value);
    }
}
