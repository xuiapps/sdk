using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using Xui.Core.Actual;
using static Xui.Runtime.IOS.ObjC;
using static Xui.Runtime.IOS.CoreFoundation;

namespace Xui.Runtime.IOS.CoreHaptics;

/// <summary>Owns a Core Haptics engine, player and reusable dynamic parameters.</summary>
internal sealed unsafe partial class HapticSession : IDisposable
{
    private static readonly nint Library = NativeLibrary.Load("/System/Library/Frameworks/CoreHaptics.framework/CoreHaptics");
    private static readonly Class EngineClass = new(Library, "CHHapticEngine");
    private static readonly Class PatternClass = new(Library, "CHHapticPattern");
    private static readonly Class DynamicClass = new(Library, "CHHapticDynamicParameter");
    private static readonly Class ArrayClass = new(ObjC.Lib, "NSArray");
    private static readonly Class DataClass = new(ObjC.Lib, "NSData");
    private static readonly Class JsonClass = new(ObjC.Lib, "NSJSONSerialization");
    private static readonly Sel StartEngineSel = new("startAndReturnError:");
    private static readonly Sel CancelSel = new("cancelAndReturnError:");
    private static readonly Sel StartSel = new("startAtTime:error:");
    private static readonly Sel SendSel = new("sendParameters:atTime:error:");
    private static readonly Sel ValueSel = new("setValue:");
    private static readonly Sel MutedSel = new("setIsMuted:");
    private nint engine, player, strengthParameter, frequencyParameter, parameters;
    private bool controlled;

    public static bool IsSupported
    {
        get
        {
            using var pool = new Pool();
            var capabilities = objc_msgSend_retIntPtr(EngineClass, new Sel("capabilitiesForHardware"));
            return objc_msgSend_retBool(capabilities, new Sel("supportsHaptics"));
        }
    }

    public HapticSession()
    {
        using var pool = new Pool();
        nint error = 0;
        engine = objc_msgSend_retIntPtr(EngineClass.Alloc(), new Sel("initAndReturnError:"), (nint)(&error));
        if (engine == 0) throw new InvalidOperationException("Core Haptics engine could not be created.");
        try
        {
            objc_msgSend(engine, new Sel("setPlaysHapticsOnly:"), true);
            Check(CallError(engine, StartEngineSel, &error), error);
            var initializer = new Sel("initWithParameterID:value:relativeTime:");
            strengthParameter = Parameter(DynamicClass.Alloc(), initializer, Constant("CHHapticDynamicParameterIDHapticIntensityControl"), 0, 0);
            frequencyParameter = Parameter(DynamicClass.Alloc(), initializer, Constant("CHHapticDynamicParameterIDHapticSharpnessControl"), 0, 0);
            nint* items = stackalloc nint[] { strengthParameter, frequencyParameter };
            parameters = Array(ArrayClass.Alloc(), new Sel("initWithObjects:count:"), items, 2);
        }
        catch { Dispose(); throw; }
    }

    public void Control(NFloat strength, NFloat frequency)
    {
        using var pool = new Pool();
        nint error = 0;
        if (!controlled || player == 0)
        {
            CreatePlayer([], true, true);
            objc_msgSend(player, MutedSel, true);
            Check(AtTime(player, StartSel, 0, &error), error);
            controlled = true;
        }
        SetFloat(strengthParameter, ValueSel, (float)strength);
        SetFloat(frequencyParameter, ValueSel, (float)frequency);
        Check(Send(player, SendSel, parameters, 0, &error), error);
        objc_msgSend(player, MutedSel, false);
    }

    public void Play(ReadOnlySpan<VibrationPoint> points, bool repeat)
    {
        using var pool = new Pool();
        CreatePlayer(points, repeat, false);
        nint error = 0;
        Check(AtTime(player, StartSel, 0, &error), error);
    }

    private void CreatePlayer(ReadOnlySpan<VibrationPoint> points, bool repeat, bool continuous)
    {
        Stop();
        using var stream = new MemoryStream();
        using (var json = new Utf8JsonWriter(stream))
        {
            json.WriteStartObject(); json.WriteNumber("Version", 1);
            json.WriteStartArray("Pattern");
            json.WriteStartObject(); json.WriteStartObject("Event");
            json.WriteString("EventType", "HapticContinuous"); json.WriteNumber("Time", 0);
            json.WriteNumber("EventDuration", continuous ? 30 : points[^1].Time.TotalSeconds);
            json.WriteStartArray("EventParameters");
            WriteParameter(json, "HapticIntensity", 1); WriteParameter(json, "HapticSharpness", 0);
            json.WriteEndArray(); json.WriteEndObject(); json.WriteEndObject();
            if (!continuous)
            {
                WriteCurve(json, points, "HapticIntensityControl", true);
                WriteCurve(json, points, "HapticSharpnessControl", false);
            }
            json.WriteEndArray(); json.WriteEndObject();
        }
        var bytes = stream.GetBuffer();
        nint error = 0;
        fixed (byte* data = bytes)
        {
            var nsData = Data(DataClass, new Sel("dataWithBytes:length:"), data, (nuint)stream.Length);
            var dictionary = Json(JsonClass, new Sel("JSONObjectWithData:options:error:"), nsData, 0, &error);
            if (dictionary == 0) throw new InvalidOperationException("Invalid Core Haptics pattern JSON.");
            var pattern = objc_msgSend_retIntPtr(PatternClass.Alloc(), new Sel("initWithDictionary:error:"), dictionary, (nint)(&error));
            if (pattern == 0) throw new InvalidOperationException("Core Haptics rejected the pattern.");
            try
            {
                player = objc_msgSend_retIntPtr(engine, new Sel("createAdvancedPlayerWithPattern:error:"), pattern, (nint)(&error));
                if (player == 0) throw new InvalidOperationException("Core Haptics could not create a player.");
                CFRetain(player); // Factory returns a borrowed/autoreleased player.
                objc_msgSend(player, new Sel("setLoopEnabled:"), repeat);
                SetDouble(player, new Sel("setLoopEnd:"), continuous ? 30 : points[^1].Time.TotalSeconds);
            }
            finally { CFRelease(pattern); }
        }
    }

    private static void WriteParameter(Utf8JsonWriter json, string id, double value)
    {
        json.WriteStartObject(); json.WriteString("ParameterID", id);
        json.WriteNumber("ParameterValue", value); json.WriteEndObject();
    }

    private static void WriteCurve(Utf8JsonWriter json, ReadOnlySpan<VibrationPoint> points, string id, bool strength)
    {
        // Core Haptics accepts at most 16 points per curve. Consecutive curves
        // share an endpoint, with point times relative to each curve's start.
        for (var start = 0; start < points.Length - 1; start += 15)
        {
            var offset = points[start].Time;
            json.WriteStartObject(); json.WriteStartObject("ParameterCurve");
            json.WriteString("ParameterID", id); json.WriteNumber("Time", offset.TotalSeconds);
            json.WriteStartArray("ParameterCurveControlPoints");
            foreach (var point in points.Slice(start, Math.Min(16, points.Length - start)))
            {
                json.WriteStartObject(); json.WriteNumber("Time", (point.Time - offset).TotalSeconds);
                json.WriteNumber("ParameterValue", (double)(strength ? point.Strength : point.Frequency));
                json.WriteEndObject();
            }
            json.WriteEndArray(); json.WriteEndObject(); json.WriteEndObject();
        }
    }

    public void Stop()
    {
        controlled = false;
        if (player == 0) return;
        nint error = 0;
        CallError(player, CancelSel, &error);
        CFRelease(player); player = 0;
    }

    public void Dispose()
    {
        using var pool = new Pool();
        Stop();
        if (parameters != 0) { CFRelease(parameters); parameters = 0; }
        if (strengthParameter != 0) { CFRelease(strengthParameter); strengthParameter = 0; }
        if (frequencyParameter != 0) { CFRelease(frequencyParameter); frequencyParameter = 0; }
        if (engine != 0)
        {
            objc_msgSend(engine, new Sel("stopWithCompletionHandler:"), (nint)0);
            CFRelease(engine); engine = 0;
        }
    }

    private static nint Constant(string name) => Marshal.ReadIntPtr(NativeLibrary.GetExport(Library, name));
    private static void Check(bool succeeded, nint error)
    {
        if (!succeeded) throw new InvalidOperationException($"Core Haptics command failed (NSError {error}).");
    }

    private readonly ref struct Pool : IDisposable
    {
        private readonly nint value;
        public Pool() => value = PoolPush();
        public void Dispose() => PoolPop(value);
    }

    [LibraryImport(LibObjCLib, EntryPoint = "objc_autoreleasePoolPush")] private static partial nint PoolPush();
    [LibraryImport(LibObjCLib, EntryPoint = "objc_autoreleasePoolPop")] private static partial void PoolPop(nint pool);
    [LibraryImport(LibObjCLib, EntryPoint = "objc_msgSend")] [return: MarshalAs(UnmanagedType.I1)]
    private static partial bool CallError(nint obj, nint sel, nint* error);
    [LibraryImport(LibObjCLib, EntryPoint = "objc_msgSend")] [return: MarshalAs(UnmanagedType.I1)]
    private static partial bool AtTime(nint obj, nint sel, double time, nint* error);
    [LibraryImport(LibObjCLib, EntryPoint = "objc_msgSend")] [return: MarshalAs(UnmanagedType.I1)]
    private static partial bool Send(nint obj, nint sel, nint parameters, double time, nint* error);
    [LibraryImport(LibObjCLib, EntryPoint = "objc_msgSend")]
    private static partial nint Parameter(nint obj, nint sel, nint id, float value, double time);
    [LibraryImport(LibObjCLib, EntryPoint = "objc_msgSend")]
    private static partial void SetFloat(nint obj, nint sel, float value);
    [LibraryImport(LibObjCLib, EntryPoint = "objc_msgSend")]
    private static partial void SetDouble(nint obj, nint sel, double value);
    [LibraryImport(LibObjCLib, EntryPoint = "objc_msgSend")]
    private static partial nint Array(nint obj, nint sel, nint* items, nuint count);
    [LibraryImport(LibObjCLib, EntryPoint = "objc_msgSend")]
    private static partial nint Data(nint obj, nint sel, byte* bytes, nuint length);
    [LibraryImport(LibObjCLib, EntryPoint = "objc_msgSend")]
    private static partial nint Json(nint obj, nint sel, nint data, nuint options, nint* error);
}
