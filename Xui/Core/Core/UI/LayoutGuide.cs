using Xui.Core.Canvas;
using Xui.Core.Debug;
using Xui.Core.Math2D;

namespace Xui.Core.UI;

/// <summary>
/// Encapsulates the parameters and results of a layout pass (Measure, Arrange, Render) for a view.
/// </summary>
public struct LayoutGuide
{
    /// <summary>
    /// Indicates the type of layout pass being performed: Measure, Arrange, or Render.
    /// </summary>
    public LayoutPass Pass;

    /// <summary>
    /// The time of the previous animation frame.
    /// </summary>
    public TimeSpan PreviousTime;

    /// <summary>
    /// The time of the current animation frame for this view.
    /// For Measure/Arrange/Render passes, this may be <see cref="TimeSpan.Zero"/>.
    /// </summary>
    public TimeSpan CurrentTime;

    // Measure spec

    /// <summary>
    /// The available space for measuring this view's margin box. Used during the Measure pass.
    /// </summary>
    public Size AvailableSize;

    /// <summary>
    /// How the view should size itself horizontally during measurement (exact or at-most).
    /// </summary>
    public SizeTo XSize;

    /// <summary>
    /// How the view should size itself vertically during measurement (exact or at-most).
    /// </summary>
    public SizeTo YSize;

    /// <summary>
    /// Optional measurement context providing access to platform-specific text metrics
    /// and precise size calculations during the Measure pass.
    /// If set, text and layout measurements can use font shaping and pixel snapping
    /// consistent with the underlying rendering system.
    /// </summary>
    public IMeasureContext? MeasureContext;

    /// <summary>
    /// The desired size of the view's margin box, produced during the Measure pass.
    /// </summary>
    public Size DesiredSize;

    // Arrange spec

    /// <summary>
    /// The anchor point that defines the alignment constraint for layout.
    /// This point serves as a reference for positioning the view based on alignment.
    /// For example, if alignment is set to <see cref="Align.End"/>, the anchor represents the bottom-right constraint.
    /// If alignment is <see cref="Align.Start"/>, it represents the top-left constraint.
    /// </summary>
    public Point Anchor;

    /// <summary>
    /// The horizontal alignment of the view within its allocated space.
    /// </summary>
    public Align XAlign;

    /// <summary>
    /// The vertical alignment of the view within its allocated space.
    /// </summary>
    public Align YAlign;

    /// <summary>
    /// The final rectangle occupied by the view's border edge box after the Arrange pass.
    /// </summary>
    public Rect ArrangedRect;

    // Render spec

    /// <summary>
    /// Optional rendering context for drawing during the Render pass.
    /// </summary>
    public IContext? RenderContext;

    /// <summary>
    /// Instrumentation accessor for zero-alloc logging during layout passes.
    /// </summary>
    public InstrumentsAccessor Instruments;

    /// <summary>
    /// Returns true if this guide represents an animation pass.
    /// </summary>
    public bool IsAnimate => (this.Pass & LayoutPass.Animate) == LayoutPass.Animate;

    /// <summary>
    /// Returns true if this guide represents a Measure pass.
    /// </summary>
    public bool IsMeasure => (this.Pass & LayoutPass.Measure) == LayoutPass.Measure;

    /// <summary>
    /// Returns true if this guide represents an Arrange pass.
    /// </summary>
    public bool IsArrange => (this.Pass & LayoutPass.Arrange) == LayoutPass.Arrange;

    /// <summary>
    /// Returns true if this guide represents a Render pass.
    /// </summary>
    public bool IsRender => (this.Pass & LayoutPass.Render) == LayoutPass.Render;

    /// <summary>
    /// Returns true if all four passes are requested (Animate, Measure, Arrange, Render),
    /// meaning the view can process the full pipeline in a single DFS walk via <see cref="View.ForkUpdate"/>.
    /// </summary>
    public bool IsLuminarFlow => this.Pass == LayoutPass.LuminarFlow;

    /// <summary>
    /// Flags indicating which type of layout pass is being performed.
    /// Multiple passes may be combined (e.g., Measure | Render).
    /// </summary>
    [Flags]
    public enum LayoutPass : byte
    {
        /// <summary>
        /// Indicates an animation timing pass. Views can update time-based state
        /// for the current frame (e.g., tweens). Typically runs before layout/render.
        /// </summary>
        Animate = 1 << 0,

        /// <summary>
        /// Indicates a Measure pass to determine desired size.
        /// </summary>
        Measure = 1 << 1,

        /// <summary>
        /// Indicates an Arrange pass to finalize layout position and size.
        /// </summary>
        Arrange = 1 << 2,

        /// <summary>
        /// Indicates a Render pass to draw the view's content.
        /// </summary>
        Render = 1 << 3,

        /// <summary>
        /// All four passes combined: Animate, Measure, Arrange, and Render.
        /// When a guide carries this value, <see cref="View.ForkUpdate"/> is eligible for
        /// a single-pass DFS traversal instead of four separate child walks.
        /// </summary>
        LuminarFlow = Animate | Measure | Arrange | Render,
    }

    /// <summary>
    /// Defines how the view should interpret the size constraints during measurement.
    /// </summary>
    public enum SizeTo : byte
    {
        /// <summary>
        /// The view must exactly match the given size constraints.
        /// </summary>
        Exact,

        /// <summary>
        /// The view may size to its content, but must not exceed the given constraints.
        /// </summary>
        AtMost
    }

    /// <summary>
    /// Defines alignment of a view within a layout axis.
    /// </summary>
    public enum Align : byte
    {
        /// <summary>
        /// Align to the start (top or left).
        /// </summary>
        Start = 0,

        /// <summary>
        /// Align to the center.
        /// </summary>
        Center = 1,

        /// <summary>
        /// Align to the end (bottom or right).
        /// </summary>
        End = 2
    }
}
