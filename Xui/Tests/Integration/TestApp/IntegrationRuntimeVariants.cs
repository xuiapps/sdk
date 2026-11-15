using Xui.Core.Abstract;
using Xui.Core.Math2D;
using Xui.Middleware.EmulatorLegacy.Devices;
using Xui.Runtime.Test;
using Microsoft.Extensions.DependencyInjection;

namespace Xui.Tests.Integration.TestApp;

public static class IntegrationRuntimeVariants
{
    public static IEnumerable<object[]> All
    {
        get
        {
            yield return [TestRuntimeVariant.Desktop];
            yield return [TestRuntimeVariant.IPhoneEmulator];
        }
    }

    public static IEnumerable<object[]> PhoneFormFactors
    {
        get
        {
            yield return [TestRuntimeVariant.IPhoneEmulator];
            yield return [TestRuntimeVariant.IPhoneNotchEmulator];
            yield return [TestRuntimeVariant.AndroidPunchHoleEmulator];
            yield return [TestRuntimeVariant.AndroidWaterdropEmulator];
            yield return [TestRuntimeVariant.AndroidNoCutoutEmulator];
        }
    }

    public static TestSinglePageApp<TApplication, TWindow> CreateApp<TApplication, TWindow>(
        Size desktopSize,
        TestRuntimeVariant runtimeVariant,
        string callerPath,
        string testName)
        where TApplication : Application
        where TWindow : Window
    {
        var emulatorDevice = ResolveEmulatorDevice(runtimeVariant);
        var size = emulatorDevice is null
            ? desktopSize
            : new Size(
                emulatorDevice.Value.LogicalResolution.Width + 48,
                emulatorDevice.Value.LogicalResolution.Height + 64);
        var snapshotSet = ResolveSnapshotSet(runtimeVariant);

        return new TestSinglePageApp<TApplication, TWindow>(
            size,
            configure: services =>
            {
                var settings = new Xui.Apps.TestApp.TestSettings();
                services.AddSingleton<Xui.Apps.TestApp.ITestSettings>(settings);
                services.AddSingleton<IRandom>(_ => settings.RandomSeed is { } seed
                    ? new SeededRandom(seed)
                    : SystemRandom.Default);
            },
            runtimeVariant: runtimeVariant,
            emulatorDevice: emulatorDevice,
            snapshotSet: snapshotSet,
            callerPath: callerPath,
            testName: testName);
    }

    private static string? ResolveSnapshotSet(TestRuntimeVariant runtimeVariant) =>
        runtimeVariant switch
        {
            TestRuntimeVariant.IPhoneEmulator => "Scenarios.Emulator.iPhone15Pro",
            TestRuntimeVariant.IPhoneNotchEmulator => "Scenarios.Emulator.iPhone14",
            TestRuntimeVariant.AndroidPunchHoleEmulator => "Scenarios.Emulator.pixel8Pro",
            TestRuntimeVariant.AndroidWaterdropEmulator => "Scenarios.Emulator.galaxyA54",
            TestRuntimeVariant.AndroidNoCutoutEmulator => "Scenarios.Emulator.redMagic9Pro",
            _ => null
        };

    private static DeviceProfile? ResolveEmulatorDevice(TestRuntimeVariant runtimeVariant) =>
        runtimeVariant switch
        {
            TestRuntimeVariant.IPhoneEmulator => FindDevice("iPhone 15 Pro"),
            TestRuntimeVariant.IPhoneNotchEmulator => FindDevice("iPhone 14"),
            TestRuntimeVariant.AndroidPunchHoleEmulator => FindDevice("Pixel 8 Pro"),
            TestRuntimeVariant.AndroidWaterdropEmulator => FindDevice("Galaxy A54"),
            TestRuntimeVariant.AndroidNoCutoutEmulator => FindDevice("RedMagic 9 Pro"),
            _ => null
        };

    private static DeviceProfile FindDevice(string model)
    {
        var index = DeviceCatalog.All.FindIndex(d => d.Model == model);
        if (index < 0)
            throw new InvalidOperationException($"Missing emulator profile '{model}'.");
        return DeviceCatalog.All[index];
    }
}
