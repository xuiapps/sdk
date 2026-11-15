namespace Xui.Apps.TestApp.Examples;

/// <summary>Allows an example to replace its desktop composition when hosted on a phone.</summary>
internal interface IMobileExamplePresentation
{
    void UseMobilePresentation();

    /// <summary>Allows scrolling content to paint beneath the shell header and system safe areas.</summary>
    bool DrawsUnderHeader => false;
}
