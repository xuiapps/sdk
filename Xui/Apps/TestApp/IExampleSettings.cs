namespace Xui.Apps.TestApp;

/// <summary>Shared example preferences supplied by the application view root.</summary>
public interface IExampleSettings
{
    /// <summary>Raised when an example preference changes.</summary>
    event Action? Changed;

    /// <summary>Use reusable drawing symbols in the floorplan; enabled by default.</summary>
    bool UseFloorplanSymbols { get; set; }
}
