namespace Xui.Compiler.XuiNet.DotNet.Exceptions;

/// <summary>Built-in managed exception identities supported without compiling the BCL implementation.</summary>
public enum ExceptionKind
{
    Exception = 1,
    SystemException,
    InvalidCastException,
    NullReferenceException,
    InvalidOperationException,
    ArgumentException,
    IndexOutOfRangeException,
    OverflowException,
    ArgumentNullException
}
