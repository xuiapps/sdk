using System;
using Xui.Core.Actual;
using static Xui.Runtime.IOS.UIKit;

namespace Xui.Runtime.IOS.Actual;

/// <summary>Window-owned generators, allocated lazily and reused across interactions.</summary>
internal sealed class IOSHapticFeedback : IHapticFeedback, IDisposable
{
    private UISelectionFeedbackGenerator? selection;
    private UINotificationFeedbackGenerator? notification;
    private readonly UIImpactFeedbackGenerator?[] impacts = new UIImpactFeedbackGenerator?[5];

    public void Prepare(HapticEffect effect)
    {
        switch (effect)
        {
            case HapticEffect.Tick:
                (selection ??= new()).Prepare();
                break;
            case HapticEffect.LightImpact or HapticEffect.MediumImpact or HapticEffect.HeavyImpact
                or HapticEffect.SoftImpact or HapticEffect.RigidImpact:
                Impact(effect).Prepare();
                break;
            case HapticEffect.Success or HapticEffect.Warning or HapticEffect.Error:
                (notification ??= new()).Prepare();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(effect));
        }
    }

    public void Play(HapticEffect effect)
    {
        switch (effect)
        {
            case HapticEffect.Tick:
                (selection ??= new()).SelectionChanged();
                break;
            case HapticEffect.LightImpact or HapticEffect.MediumImpact or HapticEffect.HeavyImpact
                or HapticEffect.SoftImpact or HapticEffect.RigidImpact:
                Impact(effect).ImpactOccurred();
                break;
            case HapticEffect.Success or HapticEffect.Warning or HapticEffect.Error:
                (notification ??= new()).NotificationOccurred(effect switch
                {
                    HapticEffect.Success => UINotificationFeedbackGenerator.FeedbackType.Success,
                    HapticEffect.Warning => UINotificationFeedbackGenerator.FeedbackType.Warning,
                    _ => UINotificationFeedbackGenerator.FeedbackType.Error,
                });
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(effect));
        }
        Prepare(effect);
    }

    private UIImpactFeedbackGenerator Impact(HapticEffect effect)
    {
        var index = (int)effect - (int)HapticEffect.LightImpact;
        return impacts[index] ??= new(effect switch
        {
            HapticEffect.LightImpact => UIImpactFeedbackGenerator.FeedbackStyle.Light,
            HapticEffect.MediumImpact => UIImpactFeedbackGenerator.FeedbackStyle.Medium,
            HapticEffect.HeavyImpact => UIImpactFeedbackGenerator.FeedbackStyle.Heavy,
            HapticEffect.SoftImpact => UIImpactFeedbackGenerator.FeedbackStyle.Soft,
            _ => UIImpactFeedbackGenerator.FeedbackStyle.Rigid,
        });
    }

    public void Dispose()
    {
        selection?.Dispose(); selection = null;
        notification?.Dispose(); notification = null;
        for (var i = 0; i < impacts.Length; i++)
        {
            impacts[i]?.Dispose(); impacts[i] = null;
        }
    }
}
