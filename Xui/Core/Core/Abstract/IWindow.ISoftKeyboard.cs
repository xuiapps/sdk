using Xui.Core.Abstract.Events;
using Xui.Core.Math2D;

namespace Xui.Core.Abstract;

public partial interface IWindow
{
    /// <summary>
    /// Represents a handler for software keyboard input.
    /// </summary>
    /// <remarks>
    /// This interface allows focused views to receive input from the system's software keyboard,
    /// typically used on mobile platforms and the emulator. It handles character insertion
    /// and backward deletion commands.
    ///
    /// This is not meant to expose full IME or keyboard layout logic—just basic input dispatch.
    /// </remarks>
    public interface ISoftKeyboard
    {
        /// <summary>Whether the focused text client contains text.</summary>
        bool HasText => false;

        /// <summary>Whether the focused text client requires password entry.</summary>
        bool IsSecureTextEntry => false;

        /// <summary>
        /// Requests insertion of one or more characters into the current input context.
        /// </summary>
        /// <param name="eventRef">The input event containing the text to insert.</param>
        public void InsertText(ref InsertTextEventRef eventRef);

        /// <summary>
        /// Requests deletion of content preceding the caret or selection.
        /// </summary>
        /// <param name="eventRef">The input event representing a backspace action.</param>
        public void DeleteBackwards(ref DeleteBackwardsEventRef eventRef);
    }
}
