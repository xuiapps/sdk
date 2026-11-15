using Android.App;
using Android.Content.PM;
using Android.OS;
using Xui.Runtime.Android.Actual;

namespace Xui.Apps.TestApp;

[Activity(MainLauncher = true, Theme = "@android:style/Theme.Material.Light.NoActionBar",
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode |
        ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public sealed class MainActivity : XuiActivity
{
    protected override void OnCreate(Bundle? state)
    {
        base.OnCreate(state);
        TestAppBootstrap.Run();
    }
}
