using Xui.Core.Set;

namespace Xui.Core.Math2D;

/// <summary>
/// Represents a rectangle in 2D space, defined by its origin (<see cref="X"/>, <see cref="Y"/>)
/// and its dimensions (<see cref="Width"/>, <see cref="Height"/>).
/// </summary>
/// <remarks>
/// Unlike <see cref="Frame"/>, which represents edge thicknesses around a box, 
/// <see cref="Rect"/> defines a positioned, sized area used for layout and rendering.
/// </remarks>
public struct Rect : INonEnumerableSet<Point>, IEquatable<Rect>
{
    /// <summary>The X-coordinate of the rectangle’s top-left corner.</summary>
    public nfloat X;

    /// <summary>The Y-coordinate of the rectangle’s top-left corner.</summary>
    public nfloat Y;

    /// <summary>The width of the rectangle.</summary>
    public nfloat Width;

    /// <summary>The height of the rectangle.</summary>
    public nfloat Height;

    /// <summary>
    /// Creates a new <see cref="Rect"/> from position and size.
    /// </summary>
    [DebuggerStepThrough]
    public Rect(nfloat x, nfloat y, nfloat width, nfloat height)
    {
        this.X = x;
        this.Y = y;
        this.Width = width;
        this.Height = height;
    }

    /// <summary>
    /// Creates a new <see cref="Rect"/> from a top-left point and a size.
    /// </summary>
    [DebuggerStepThrough]
    public Rect(Point topLeft, Size size)
    {
        this.X = topLeft.X;
        this.Y = topLeft.Y;
        this.Width = size.Width;
        this.Height = size.Height;
    }

    /// <summary>
    /// Creates a new <see cref="Rect"/> from a top-left vector and a size.
    /// </summary>
    [DebuggerStepThrough]
    public Rect(Vector topLeft, Size size)
    {
        this.X = topLeft.X;
        this.Y = topLeft.Y;
        this.Width = size.Width;
        this.Height = size.Height;
    }

    /// <summary>Returns the top-left point of the rectangle.</summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public Point TopLeft => new (X, Y);

    /// <summary>Returns the top-center point of the rectangle.</summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public Point TopCenter => new (X + this.Width / (nfloat)2, Y);

    /// <summary>Returns the top-right point of the rectangle.</summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public Point TopRight => new (X + Width, Y);

    /// <summary>Returns the bottom-right point of the rectangle.</summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public Point BottomRight => new (X + Width, Y + Height);

    /// <summary>Returns the bottom-left point of the rectangle.</summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public Point BottomLeft => new (X, Y + Height);

    /// <summary>Returns the X-coordinate of the left edge.</summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public nfloat Left => this.X;

    /// <summary>Returns the Y-coordinate of the top edge.</summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public nfloat Top => this.Y;

    /// <summary>Returns the X-coordinate of the right edge.</summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public nfloat Right => this.X + this.Width;

    /// <summary>Returns the Y-coordinate of the bottom edge.</summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public nfloat Bottom => this.Y + this.Height;

    /// <summary>
    /// Returns the center point of the rectangle.
    /// </summary>
    public Point Center => new(X + Width / (nfloat)2, Y + Height / (nfloat)2);

    /// <summary>
    /// Gets or sets the size of the rectangle.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public Size Size
    {
        get => new (this.Width, this.Height);
        set
        {
            this.Width = value.Width;
            this.Height = value.Height;
        }
    }

    /// <summary>
    /// Returns a new rectangle that is inset by the same amount on all four sides.
    /// </summary>
    [DebuggerStepThrough]
    public Rect Inset(nfloat inset) =>
        new (
            X + inset,
            Y + inset,
            Width - inset - inset,
            Height - inset - inset
        );

    /// <summary>
    /// Returns true if the specified <paramref name="point"/> lies within this rectangle.
    /// </summary>
    [DebuggerStepThrough]
    public bool Contains(Point point) =>
        point.X >= this.X &&
        point.Y >= this.Y &&
        point.X <= this.X + this.Width &&
        point.Y <= this.Y + this.Height;

    /// <summary>Returns whether this rectangle overlaps or touches <paramref name="other"/>.</summary>
    [DebuggerStepThrough]
    public bool Intersects(Rect other) =>
        this.Left <= other.Right &&
        this.Right >= other.Left &&
        this.Top <= other.Bottom &&
        this.Bottom >= other.Top;

    /// <summary>
    /// Returns a rectangle expanded horizontally and vertically by the specified amounts.
    /// </summary>
    [DebuggerStepThrough]
    public Rect Expand(nfloat h, nfloat v) =>
        new (
            X - h,
            Y - v,
            Width + h + h,
            Height + v + v
        );

    /// <summary>
    /// Returns a rectangle expanded uniformly in all directions.
    /// </summary>
    [DebuggerStepThrough]
    public Rect Expand(nfloat expand) =>
        new (
            X - expand,
            Y - expand,
            Width + expand + expand,
            Height + expand + expand
        );

    /// <summary>
    /// Implicit conversion from a 4-tuple to a rectangle.
    /// </summary>
    [DebuggerStepThrough]
    public static implicit operator Rect(ValueTuple<nfloat, nfloat, nfloat, nfloat> tuple) => new (tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4);

    /// <summary>
    /// Returns a rectangle expanded outward by a <see cref="Frame"/>.
    /// </summary>
    [DebuggerStepThrough]
    public static Rect operator+(Rect rect, Frame frame) =>
        new Rect(rect.X - frame.Left, rect.Y - frame.Top, rect.Width + frame.Left + frame.Right, rect.Height + frame.Top + frame.Bottom);        

    /// <summary>
    /// Returns a rectangle inset inward by a <see cref="Frame"/>.
    /// </summary>
    [DebuggerStepThrough]
    public static Rect operator-(Rect rect, Frame frame) =>
        new Rect(rect.X + frame.Left, rect.Y + frame.Top, rect.Width - frame.Left - frame.Right, rect.Height - frame.Top - frame.Bottom);

    /// <summary>
    /// Returns true if two rectangles are equal in position and size.
    /// </summary>
    [DebuggerStepThrough]
    public static bool operator==(Rect lhs, Rect rhs) =>
        lhs.X == rhs.X && lhs.Y == rhs.Y && lhs.Width == rhs.Width && lhs.Height == rhs.Height;

    /// <summary>
    /// Returns true if any of the rectangle fields are different.
    /// </summary>
    [DebuggerStepThrough]
    public static bool operator!=(Rect lhs, Rect rhs) =>
        lhs.X != rhs.X || lhs.Y != rhs.Y || lhs.Width != rhs.Width || lhs.Height != rhs.Height;

    /// <inheritdoc/>
    public override bool Equals(object? obj) =>
        obj is Rect other && this == other;

    /// <summary>
    /// Returns true if this rectangle is equal to another rectangle in position and size.
    /// </summary>
    public bool Equals(Rect other) =>
        this == other;

    /// <inheritdoc/>
    public override int GetHashCode() =>
        HashCode.Combine(X, Y, Width, Height);

    /// <inheritdoc/>
    [DebuggerStepThrough]
    public override string ToString() =>
        $"Rect [X={X}, Y={Y}, Width={Width}, Height={Height}]";
}
