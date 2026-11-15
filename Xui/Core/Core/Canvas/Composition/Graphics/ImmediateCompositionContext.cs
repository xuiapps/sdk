namespace Xui.Core.Canvas.Composition.Graphics;

/// <summary>
/// Explicit immediate-mode implementation for backends without retained composition.
/// Groups isolate Save/Restore drawing state and apply presentation, but always redraw.
/// Surface effects use portable approximations; this does not create native views or retain pixels.
/// </summary>
public sealed class ImmediateCompositionContext(IContext context) : ICompositionContext
{
    private int depth;

    /// <inheritdoc/>
    public CompositionGroup Group(scoped in GroupOptions options)
    {
        options.Validate();
        context.Save();
        try
        {
            if (!options.Transform.IsIdentity)
                context.Transform(options.Transform);
            SurfaceEffects.Draw(context, in options);
            if (options.ClipToBounds)
            {
                context.BeginPath();
                context.RoundRect(options.Bounds!.Value, options.CornerRadius);
                context.Clip();
                context.BeginPath();
            }
            depth++;
            return new CompositionGroup(this, needsRedraw: true);
        }
        catch
        {
            context.Restore();
            throw;
        }
    }

    /// <inheritdoc/>
    public void EndGroup()
    {
        if (depth == 0)
            throw new InvalidOperationException("No composition group to close.");
        context.Restore();
        depth--;
    }
}
