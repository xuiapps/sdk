namespace Xui.Core.Actual;

/// <summary>
/// Tactile presets. The platform chooses the closest available native effect;
/// strength and texture are not guaranteed to be identical across devices.
/// </summary>
public enum HapticEffect
{
    /// <summary>A small, crisp ruler or picker tick.</summary>
    /// <remarks>
    /// <para>On iOS, uses <c>UISelectionFeedbackGenerator.selectionChanged()</c>.</para>
    /// <para>On Android, uses <c>View.performHapticFeedback(SEGMENT_TICK)</c> on API 34+,
    /// falling back to <c>CLOCK_TICK</c> on earlier versions.</para>
    /// </remarks>
    Tick,

    /// <summary>A light physical impact.</summary>
    /// <remarks>
    /// <para>On iOS, uses <c>UIImpactFeedbackGenerator.impactOccurred()</c> with the <c>light</c> style.</para>
    /// <para>On Android, approximated with <c>VibrationEffect.EFFECT_TICK</c> on API 29+,
    /// falling back to <c>View.performHapticFeedback(CLOCK_TICK)</c> on earlier versions.</para>
    /// </remarks>
    LightImpact,

    /// <summary>A medium physical impact.</summary>
    /// <remarks>
    /// <para>On iOS, uses <c>UIImpactFeedbackGenerator.impactOccurred()</c> with the <c>medium</c> style.</para>
    /// <para>On Android, approximated with <c>VibrationEffect.EFFECT_CLICK</c> on API 29+,
    /// falling back to <c>View.performHapticFeedback(LONG_PRESS)</c> on earlier versions.</para>
    /// </remarks>
    MediumImpact,

    /// <summary>A heavy physical impact.</summary>
    /// <remarks>
    /// <para>On iOS, uses <c>UIImpactFeedbackGenerator.impactOccurred()</c> with the <c>heavy</c> style.</para>
    /// <para>On Android, approximated with <c>VibrationEffect.EFFECT_HEAVY_CLICK</c> on API 29+,
    /// falling back to <c>View.performHapticFeedback(LONG_PRESS)</c> on earlier versions.</para>
    /// </remarks>
    HeavyImpact,

    /// <summary>A soft, rounded impact.</summary>
    /// <remarks>
    /// <para>On iOS, uses <c>UIImpactFeedbackGenerator.impactOccurred()</c> with the <c>soft</c> style.</para>
    /// <para>On Android API 31+, approximated with a composed <c>PRIMITIVE_THUD</c> at scale <c>0.5</c>
    /// when the device supports it. Otherwise uses <c>VibrationEffect.EFFECT_CLICK</c> on API 29+,
    /// or <c>View.performHapticFeedback(CLOCK_TICK)</c> on earlier versions.</para>
    /// </remarks>
    SoftImpact,

    /// <summary>A firm, sharp impact.</summary>
    /// <remarks>
    /// <para>On iOS, uses <c>UIImpactFeedbackGenerator.impactOccurred()</c> with the <c>rigid</c> style.</para>
    /// <para>On Android API 31+, approximated with a composed <c>PRIMITIVE_CLICK</c> at scale <c>1.0</c>
    /// when the device supports it. Otherwise uses <c>VibrationEffect.EFFECT_CLICK</c> on API 29+,
    /// or <c>View.performHapticFeedback(LONG_PRESS)</c> on earlier versions.</para>
    /// </remarks>
    RigidImpact,

    /// <summary>Positive completion feedback.</summary>
    /// <remarks>
    /// <para>On iOS, uses <c>UINotificationFeedbackGenerator.notificationOccurred(.success)</c>.</para>
    /// <para>On Android, uses <c>View.performHapticFeedback(CONFIRM)</c> on API 30+.
    /// Approximated with <c>VibrationEffect.EFFECT_DOUBLE_CLICK</c> on API 29,
    /// or <c>View.performHapticFeedback(LONG_PRESS)</c> on earlier versions.</para>
    /// </remarks>
    Success,

    /// <summary>A cautionary notification.</summary>
    /// <remarks>
    /// <para>On iOS, uses <c>UINotificationFeedbackGenerator.notificationOccurred(.warning)</c>.</para>
    /// <para>Android has no dedicated warning preset. Approximated with
    /// <c>VibrationEffect.EFFECT_DOUBLE_CLICK</c> on API 29+, falling back to
    /// <c>View.performHapticFeedback(LONG_PRESS)</c> on earlier versions.</para>
    /// </remarks>
    Warning,

    /// <summary>Failure or rejection feedback.</summary>
    /// <remarks>
    /// <para>On iOS, uses <c>UINotificationFeedbackGenerator.notificationOccurred(.error)</c>.</para>
    /// <para>On Android, uses <c>View.performHapticFeedback(REJECT)</c> on API 30+.
    /// Approximated with <c>VibrationEffect.EFFECT_DOUBLE_CLICK</c> on API 29,
    /// or <c>View.performHapticFeedback(LONG_PRESS)</c> on earlier versions.</para>
    /// </remarks>
    Error,
}
