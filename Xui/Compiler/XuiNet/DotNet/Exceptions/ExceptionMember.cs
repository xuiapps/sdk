namespace Xui.Compiler.XuiNet.DotNet.Exceptions;

/// <summary>Supported exception constructors and property accessors, including string argument roles.</summary>
public enum ExceptionMember
{
    DefaultConstructor,
    MessageConstructor,
    ParameterConstructor,
    ParameterMessageConstructor,
    MessageParameterConstructor,
    Message,
    ParamName,
    InnerException
}
