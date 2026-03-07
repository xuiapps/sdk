using Xui.Core.Math2D;
using Xui.Core.Canvas;

namespace Xui.Core.UI
{
    /// <summary>
    /// A view that displays a single line of styled text.
    /// </summary>
    public class Label : View
    {
        /// <summary>
        /// Gets or sets the text content displayed by the label.
        /// </summary>
        public string Text { get; set; } = "";

        /// <summary>
        /// Gets or sets the color used to fill the text.
        /// </summary>
        public Color TextColor { get; set; } = Colors.Black;

        /// <summary>
        /// Gets or sets the font family used for rendering the text.
        /// </summary>
        public string[] FontFamily { get; set; } = ["Inter"];

        /// <summary>
        /// Gets or sets the font size in points.
        /// </summary>
        public nfloat FontSize { get; set; } = 15;

        /// <summary>
        /// Gets or sets the font style (e.g., normal, italic, oblique).
        /// </summary>
        public FontStyle FontStyle { get; set; } = FontStyle.Normal;

        /// <summary>
        /// Gets or sets the font weight (e.g., normal, bold, numeric weight).
        /// </summary>
        public FontWeight FontWeight { get; set; } = FontWeight.Normal;

        /// <summary>
        /// Gets or sets the font stretch (e.g., condensed, semi-expanded etc.).
        /// </summary>
        public FontStretch FontStretch { get; set; } = FontStretch.Normal;

        /// <summary>
        /// Gets or sets the line height of the text, in user units.
        /// </summary>
        /// <remarks>
        /// If set to a numeric value, this value overrides the default line height computation from the font metrics.
        /// If set to <see cref="nfloat.NaN"/>, the line height will be automatically computed based on the font's ascender,
        /// descender, and line gap, or fallback to a platform-specific multiplier of the <see cref="Font.FontSize"/> (typically 1.2×).
        /// </remarks>
        public nfloat LineHeight { get; set; } = nfloat.NaN;

        /// <inheritdoc/>
        protected override Size MeasureCore(Size availableBorderEdgeSize, IMeasureContext context)
        {
            context.SetFont(new Font()
            {
                FontFamily = this.FontFamily,
                FontSize = this.FontSize,
                FontStyle = this.FontStyle,
                FontWeight = this.FontWeight,
                LineHeight = this.LineHeight,
                FontStretch = this.FontStretch
            });
            var textSize = context.MeasureText(this.Text);
            return textSize.Size;
        }

        /// <inheritdoc/>
        protected override void RenderCore(IContext context)
        {
            context.SetFont(new Font()
            {
                FontFamily = this.FontFamily,
                FontSize = this.FontSize,
                FontStyle = this.FontStyle,
                FontWeight = this.FontWeight,
                LineHeight = this.LineHeight,
                FontStretch = this.FontStretch
            });
            context.TextBaseline = TextBaseline.Top;
            context.TextAlign = TextAlign.Left;
            context.SetFill(this.TextColor);
            context.FillText(this.Text, this.Frame.TopLeft);
            base.RenderCore(context);
        }
    }
}
