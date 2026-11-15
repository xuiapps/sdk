using Xui.Core.Math2D;

namespace Xui.Core.Canvas.Composition.Graphics;

/// <summary>Identity, content invalidation, and presentation of an explicit canvas group.</summary>
public readonly struct GroupOptions
{
    /// <summary>A stable reference identifying this group among its siblings. Never serialized as an SVG ID.</summary>
    public required object Key { get; init; }

    /// <summary>
    /// Whether the group's graphics or descendants have changed. Defaults to true.
    /// False permits a backend to reuse content, but callers must still check NeedsRedraw.
    /// </summary>
    public bool HasUpdates { get; init; } = true;

    /// <summary>
    /// Optional content bounds in the group's coordinate system. Does not translate the
    /// origin or clip implicitly. Retained backends may use these bounds for backing storage.
    /// </summary>
    public Rect? Bounds { get; init; }

    /// <summary>Local transform concatenated with the parent transform. Defaults to identity.</summary>
    public AffineTransform Transform { get; init; } = AffineTransform.Identity;

    /// <summary>Clips drawing to Bounds, after applying Transform. Requires Bounds.</summary>
    public bool ClipToBounds { get; init; }

    /// <summary>Requests a retained native surface. Requires Bounds. Draw its content before declaring child groups.</summary>
    public bool ViewBacking { get; init; }

    /// <summary>Uniform radius of the surface outline and optional bounds clip.</summary>
    public nfloat CornerRadius { get; init; }

    /// <summary>Optional surface color, updated independently of recorded drawing content.</summary>
    public Color? Background { get; init; }

    /// <summary>Native backdrop material, approximated by a translucent surface in portable renderers.</summary>
    public BackdropMaterial Backdrop { get; init; }

    /// <summary>Optional shadow of the bounds outline, independent of the drawn content's alpha.</summary>
    public OutlineShadow? Shadow { get; init; }

    /// <summary>Optional border drawn inside the rounded bounds.</summary>
    public OutlineBorder? Border { get; init; }

    /// <summary>
    /// Keep this keyed surface in a bounded cache when omitted by culling. A parent
    /// retains at most eight hidden children; older entries release their native resources.
    /// </summary>
    public bool CacheWhenHidden { get; init; }

    /// <summary>Creates options that request drawing and preserve the parent transform.</summary>
    public GroupOptions() { }

    /// <summary>Checks required identity and bounds before a backend changes drawing state.</summary>
    public void Validate()
    {
        ArgumentNullException.ThrowIfNull(Key);
        if (Key.GetType().IsValueType)
            throw new ArgumentException("A group key must be a stable reference, not a boxed value.", nameof(Key));
        if (ClipToBounds && Bounds is null)
            throw new ArgumentException("Clipping a group requires bounds.", nameof(Bounds));
        if ((ViewBacking || Background is not null || Border is not null || Shadow is not null || Backdrop != BackdropMaterial.None) && Bounds is null)
            throw new ArgumentException("Backed groups and surface effects require bounds.", nameof(Bounds));
        if (!nfloat.IsFinite(CornerRadius) || CornerRadius < 0) throw new ArgumentOutOfRangeException(nameof(CornerRadius));
        if (!Enum.IsDefined(Backdrop)) throw new ArgumentOutOfRangeException(nameof(Backdrop));
        Shadow?.Validate();
        Border?.Validate();
        if (Bounds is { } bounds && (!nfloat.IsFinite(bounds.X) || !nfloat.IsFinite(bounds.Y) ||
            !nfloat.IsFinite(bounds.Width) || !nfloat.IsFinite(bounds.Height) || bounds.Width < 0 || bounds.Height < 0))
            throw new ArgumentOutOfRangeException(nameof(Bounds));
        if (!nfloat.IsFinite(Transform.A) || !nfloat.IsFinite(Transform.B) ||
            !nfloat.IsFinite(Transform.C) || !nfloat.IsFinite(Transform.D) ||
            !nfloat.IsFinite(Transform.Tx) || !nfloat.IsFinite(Transform.Ty))
            throw new ArgumentOutOfRangeException(nameof(Transform));
    }
}
