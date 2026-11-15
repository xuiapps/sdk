using Xui.Core.UI;

namespace Xui.Core.Actual;

/// <summary>Optional native range editing and selection service, resolved through the window.</summary>
public interface ITextInputSession
{
    /// <summary>Connects the focused editor, or ends the session on blur. The service owns NativeSelection while connected.</summary>
    void SetClient(ITextEditingClient? client);

    /// <summary>Cancels native selection gestures when an ancestor takes pointer capture.</summary>
    void CancelSelectionGesture();
}
