namespace Xui.Apps.TestApp;

/// <summary>Optional developer features for this TestApp window.</summary>
public sealed class ExampleSettings : IExampleSettings
{
    private bool showDeveloperFeatures;
    private bool useFloorplanSymbols = true;

    public event Action? Changed;

    public bool UseFloorplanSymbols
    {
        get => useFloorplanSymbols;
        set
        {
            if (useFloorplanSymbols == value) return;
            useFloorplanSymbols = value;
            Changed?.Invoke();
        }
    }

    public bool ShowDeveloperFeatures
    {
        get => showDeveloperFeatures;
        set
        {
            if (showDeveloperFeatures == value) return;
            showDeveloperFeatures = value;
            Changed?.Invoke();
        }
    }
}
