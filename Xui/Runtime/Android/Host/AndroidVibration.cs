using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Provider;
using Android.Views;
using Xui.Core.Actual;

namespace Xui.Runtime.Android.Actual;

/// <summary>Native envelopes where supported; amplitude waveforms elsewhere.</summary>
internal sealed class AndroidVibration : IVibration, IDisposable
{
    private readonly View view;
    private readonly Vibrator? vibrator;
    private readonly Handler handler = new(Looper.MainLooper!);
    private readonly Java.Lang.Runnable update;
    private bool pending, playing, disposed;
    private NFloat strength, frequency;
    private int lastStrength = -1, lastFrequency = -1;

    public AndroidVibration(View view)
    {
        this.view = view;
        update = new Java.Lang.Runnable(FlushControl);
        if (OperatingSystem.IsAndroidVersionAtLeast(31))
        {
            using var manager = (VibratorManager?)view.Context!.GetSystemService(Context.VibratorManagerService);
            vibrator = manager?.DefaultVibrator;
        }
        else vibrator = (Vibrator?)view.Context!.GetSystemService(Context.VibratorService);
        IsSupported = vibrator?.HasVibrator == true;
        SupportsFrequency = IsSupported && OperatingSystem.IsAndroidVersionAtLeast(36) && vibrator!.AreEnvelopeEffectsSupported();
    }

    public bool IsSupported { get; }
    public bool SupportsFrequency { get; }

    public void Play(NFloat strength, NFloat frequency)
    {
        VibrationPattern.Validate(strength, frequency);
        if (strength == 0) { Stop(); return; }
        this.strength = strength; this.frequency = frequency;
        if (disposed || !IsSupported || pending) return;
        if (!playing) FlushControl();
        else
        {
            // Android replaces entire effects. Coalesce rapid pointer updates to
            // avoid repeatedly restarting the envelope before its attack completes.
            pending = true;
            handler.PostDelayed(update, 40);
        }
    }

    private void FlushControl()
    {
        pending = false;
        if (disposed) return;
        var s = (int)NFloat.Round(strength * 100);
        var f = SupportsFrequency ? (int)NFloat.Round(frequency * 100) : 0;
        if (s == lastStrength && f == lastFrequency) return;
        StopNative();
        if (!CanPlay()) return;
        if (SupportsFrequency && OperatingSystem.IsAndroidVersionAtLeast(36))
        {
            using var info = vibrator!.EnvelopeEffectInfo;
            var ramp = info!.MinControlPointDurationMillis;
            var hold = Math.Min(4000, Math.Min(info.MaxControlPointDurationMillis, info.MaxDurationMillis - 2 * ramp));
            using var builder = new VibrationEffect.BasicEnvelopeBuilder();
            builder.SetInitialSharpness((float)frequency);
            // Reach the live value quickly; a uniformly sampled long envelope
            // would restart a slow attack on every pointer update.
            builder.AddControlPoint((float)strength, (float)frequency, ramp);
            builder.AddControlPoint((float)strength, (float)frequency, hold);
            builder.AddControlPoint(0, (float)frequency, ramp);
            using var envelope = builder.Build()!;
            using var effect = VibrationEffect.CreateRepeatingEffect(envelope)!;
            Vibrate(effect);
        }
        else if (OperatingSystem.IsAndroidVersionAtLeast(26))
        {
            using var effect = VibrationEffect.CreateWaveform([1000L], [(int)NFloat.Round(strength * 255)], 0)!;
            Vibrate(effect);
        }
        else
        {
#pragma warning disable CA1422
            vibrator!.Vibrate([0L, 1000L], 0);
#pragma warning restore CA1422
            playing = true;
        }
        lastStrength = s; lastFrequency = f;
    }

    public void Play(scoped ReadOnlySpan<VibrationPoint> points, bool repeat = false)
    {
        VibrationPattern.Validate(points);
        Stop();
        if (!CanPlay()) return;
        if (OperatingSystem.IsAndroidVersionAtLeast(26))
        {
            using var effect = CreateEffect(points, repeat);
            Vibrate(effect);
        }
        else
        {
            // Legacy devices can only alternate off/on durations.
            var durations = new long[points.Length * 2];
            for (var i = 1; i < points.Length; i++)
            {
                var ms = Math.Max(1, (long)(points[i].Time - points[i - 1].Time).TotalMilliseconds);
                durations[(i - 1) * 2 + (points[i - 1].Strength > 0 ? 1 : 0)] = ms;
            }
#pragma warning disable CA1422
            vibrator!.Vibrate(durations, repeat ? 0 : -1);
#pragma warning restore CA1422
            playing = true;
        }
    }

    [SupportedOSPlatform("android26.0")]
    private VibrationEffect CreateEffect(ReadOnlySpan<VibrationPoint> points, bool repeat)
    {
        var duration = points[^1].Time.TotalMilliseconds;
        if (SupportsFrequency && OperatingSystem.IsAndroidVersionAtLeast(36))
        {
            using var info = vibrator!.EnvelopeEffectInfo;
            var minimum = Math.Max(20L, info!.MinControlPointDurationMillis);
            var count = Math.Min(info.MaxSize, (int)(duration / minimum));
            if (count >= 2 && duration <= info.MaxDurationMillis &&
                Math.Ceiling(duration / count) <= info.MaxControlPointDurationMillis)
            {
                // Reserve the final point for the zero-intensity release required by Android.
                using var builder = new VibrationEffect.BasicEnvelopeBuilder();
                builder.SetInitialSharpness((float)points[0].Frequency);
                var step = (long)(duration / count);
                for (var i = 1; i <= count; i++)
                {
                    var point = VibrationPattern.Sample(points, TimeSpan.FromMilliseconds(i * step));
                    builder.AddControlPoint(i == count ? 0 : (float)point.Strength, (float)point.Frequency, step);
                }
                var envelope = builder.Build()!;
                if (!repeat) return envelope;
                using (envelope) return VibrationEffect.CreateRepeatingEffect(envelope)!;
            }
        }

        var samples = Math.Max(1, (int)Math.Ceiling(duration / 20));
        var times = new long[samples]; var amplitudes = new int[samples];
        long previous = 0;
        for (var i = 0; i < samples; i++)
        {
            var end = (long)Math.Round(duration * (i + 1) / samples);
            times[i] = Math.Max(1, end - previous);
            var point = VibrationPattern.Sample(points, TimeSpan.FromMilliseconds((previous + end) / 2.0));
            amplitudes[i] = (int)NFloat.Round(point.Strength * 255);
            previous = end;
        }
        return VibrationEffect.CreateWaveform(times, amplitudes, repeat ? 0 : -1)!;
    }

    private bool CanPlay() => !disposed && IsSupported && view.HapticFeedbackEnabled && view.IsShown && view.HasWindowFocus &&
        (OperatingSystem.IsAndroidVersionAtLeast(33) || Settings.System.GetInt(view.Context!.ContentResolver, Settings.System.HapticFeedbackEnabled, 1) != 0);

    [SupportedOSPlatform("android26.0")]
    private void Vibrate(VibrationEffect effect)
    {
        if (OperatingSystem.IsAndroidVersionAtLeast(33))
        {
            using var builder = new VibrationAttributes.Builder();
            builder.SetUsage((int)VibrationAttributesUsageType.Touch);
            using var attributes = builder.Build();
            vibrator!.Vibrate(effect, attributes!);
        }
        else
        {
            using var builder = new AudioAttributes.Builder();
            builder.SetUsage(AudioUsageKind.AssistanceSonification);
            using var attributes = builder.Build();
            vibrator!.Vibrate(effect, attributes!);
        }
        playing = true;
    }

    private void StopNative()
    {
        if (playing) vibrator?.Cancel();
        playing = false;
    }

    public void Stop()
    {
        if (disposed) return;
        handler.RemoveCallbacks(update); pending = false;
        StopNative(); lastStrength = lastFrequency = -1;
    }

    public void Dispose()
    {
        if (disposed) return;
        Stop(); disposed = true;
        update.Dispose(); handler.Dispose(); vibrator?.Dispose();
    }
}
