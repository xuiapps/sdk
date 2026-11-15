namespace Xui.Apps.TestApp;

/// <summary>Optional developer features for this TestApp window.</summary>
public sealed class ExampleSettings
{
    private bool showDeveloperFeatures;

    public event Action? Changed;

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
