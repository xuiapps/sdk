namespace Xui.Compiler.Runtime;

/// <summary>Single-agent application switches owned by one compiled module instance.</summary>
/// <remarks>No process environment or desktop runtime configuration is consulted.</remarks>
public static class AppContext
{
    private static Dictionary<string, bool>? switches;

    public static void SetSwitch(string switchName, bool isEnabled)
    {
        Validate(switchName);
        switches ??= new Dictionary<string, bool>();
        switches[switchName] = isEnabled;
    }

    public static bool TryGetSwitch(string switchName, out bool isEnabled)
    {
        Validate(switchName);
        isEnabled = false;
        return switches is not null && switches.TryGetValue(switchName, out isEnabled);
    }

    private static void Validate(string switchName)
    {
        if (switchName is null) throw new ArgumentNullException(nameof(switchName));
        if (switchName.Length == 0) throw new ArgumentException("The value cannot be an empty string.", nameof(switchName));
    }
}
