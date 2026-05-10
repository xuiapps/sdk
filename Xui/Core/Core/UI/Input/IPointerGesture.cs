namespace Xui.Core.UI.Input
{
    /// <summary>
    /// Marker interface attached to a captured pointer to describe what kind of
    /// gesture the capturing view is performing. Ancestor views can inspect this
    /// (via <see cref="EventRouter.GetCapturedGesture"/>) to negotiate handover —
    /// for example a <c>ScrollView</c> will steal an <see cref="ITap"/> after a
    /// drag threshold but stand down for an <see cref="IDrag"/>.
    /// </summary>
    public interface IPointerGesture
    {
    }
}
