using System;
using System.Runtime.Versioning;
using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Provider;
using Android.Views;
using Xui.Core.Actual;

[assembly: UsesPermission(global::Android.Manifest.Permission.Vibrate)]

namespace Xui.Runtime.Android.Actual;

/// <summary>
/// Native UI feedback for ticks/results; cached vibration effects for impacts and warning.
/// Soft/rigid use supported primitives, falling back to native click effects.
/// Warning uses a double click because Android has no warning feedback constant.
/// </summary>
internal sealed class AndroidHapticFeedback(View view) : IHapticFeedback, IDisposable
{
    private readonly VibrationEffect?[] effects = new VibrationEffect?[(int)HapticEffect.Error + 1];
    private Vibrator? vibrator;
    private bool resolvedVibrator;
    private AudioAttributes? audioAttributes;
    private VibrationAttributes? vibrationAttributes;

    public void Prepare(HapticEffect effect)
    {
        Validate(effect);
        if (UsesVibration(effect) && OperatingSystem.IsAndroidVersionAtLeast(29))
            _ = Effect(effect);
    }

    public void Play(HapticEffect effect)
    {
        Validate(effect);
        if (!UsesVibration(effect))
        {
            view.PerformHapticFeedback(effect switch
            {
                HapticEffect.Tick => OperatingSystem.IsAndroidVersionAtLeast(34)
                    ? FeedbackConstants.SegmentTick : FeedbackConstants.ClockTick,
                HapticEffect.Success when OperatingSystem.IsAndroidVersionAtLeast(30) => FeedbackConstants.Confirm,
                HapticEffect.Error when OperatingSystem.IsAndroidVersionAtLeast(30) => FeedbackConstants.Reject,
                _ => FeedbackConstants.LongPress,
            });
            return;
        }

        if (!OperatingSystem.IsAndroidVersionAtLeast(29))
        {
            // Older Android versions lack predefined effects; use platform UI feedback.
            view.PerformHapticFeedback(effect is HapticEffect.LightImpact or HapticEffect.SoftImpact
                ? FeedbackConstants.ClockTick : FeedbackConstants.LongPress);
            return;
        }

        if (!view.HapticFeedbackEnabled || !view.IsShown || !view.HasWindowFocus)
            return;
        // Older Android needs an explicit preference check. From API 33, touch
        // VibrationAttributes below let the system apply current haptic settings.
        if (!OperatingSystem.IsAndroidVersionAtLeast(33) && Settings.System.GetInt(view.Context!.ContentResolver,
                Settings.System.HapticFeedbackEnabled, 1) == 0)
            return;

        var nativeEffect = Effect(effect);
        if (nativeEffect is null) return;
        if (OperatingSystem.IsAndroidVersionAtLeast(33))
        {
            if (vibrationAttributes is null)
            {
                using var builder = new VibrationAttributes.Builder();
                builder.SetUsage((int)VibrationAttributesUsageType.Touch);
                vibrationAttributes = builder.Build();
            }
            vibrator!.Vibrate(nativeEffect, vibrationAttributes);
        }
        else
        {
            if (audioAttributes is null)
            {
                using var builder = new AudioAttributes.Builder();
                builder.SetUsage(AudioUsageKind.AssistanceSonification);
                builder.SetContentType(AudioContentType.Sonification);
                audioAttributes = builder.Build();
            }
            vibrator!.Vibrate(nativeEffect, audioAttributes);
        }
    }

    private static bool UsesVibration(HapticEffect effect) => effect != HapticEffect.Tick &&
        !(OperatingSystem.IsAndroidVersionAtLeast(30) && effect is HapticEffect.Success or HapticEffect.Error);

    private static void Validate(HapticEffect effect)
    {
        if ((uint)effect > (uint)HapticEffect.Error)
            throw new ArgumentOutOfRangeException(nameof(effect));
    }

    [SupportedOSPlatform("android29.0")]
    private VibrationEffect? Effect(HapticEffect effect)
    {
        if (!resolvedVibrator)
        {
            if (OperatingSystem.IsAndroidVersionAtLeast(31))
            {
                using var manager = (VibratorManager?)view.Context!.GetSystemService(Context.VibratorManagerService);
                vibrator = manager?.DefaultVibrator;
            }
            else
                vibrator = (Vibrator?)view.Context!.GetSystemService(Context.VibratorService);
            resolvedVibrator = true;
        }
        if (vibrator?.HasVibrator != true) return null;
        var index = (int)effect;
        if (effects[index] is { } cached) return cached;

        if (OperatingSystem.IsAndroidVersionAtLeast(31) &&
            effect is HapticEffect.SoftImpact or HapticEffect.RigidImpact)
        {
            var primitive = effect == HapticEffect.SoftImpact
                ? (int)VibrationEffectCompositionPrimitive.Thud
                : (int)VibrationEffectCompositionPrimitive.Click;
            if (vibrator.ArePrimitivesSupported(primitive)?[0] == true)
            {
                using var composition = VibrationEffect.StartComposition();
                composition.AddPrimitive(primitive, effect == HapticEffect.SoftImpact ? 0.5f : 1f);
                return effects[index] = composition.Compose();
            }
        }

        return effects[index] = VibrationEffect.CreatePredefined(effect switch
        {
            HapticEffect.LightImpact => VibrationEffect.EffectTick,
            HapticEffect.HeavyImpact => VibrationEffect.EffectHeavyClick,
            HapticEffect.Success or HapticEffect.Warning or HapticEffect.Error => VibrationEffect.EffectDoubleClick,
            _ => VibrationEffect.EffectClick,
        });
    }

    public void Dispose()
    {
        foreach (var effect in effects) effect?.Dispose();
        Array.Clear(effects);
        audioAttributes?.Dispose(); audioAttributes = null;
        vibrationAttributes?.Dispose(); vibrationAttributes = null;
        vibrator?.Dispose(); vibrator = null;
    }
}
