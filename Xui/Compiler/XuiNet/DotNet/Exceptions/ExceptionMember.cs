namespace Xui.Compiler.XuiNet.DotNet.Exceptions;

/// <summary>Supported exception constructors and property accessors, including string argument roles.</summary>
public enum ExceptionMember
{
    DefaultConstructor,
    MessageInnerConstructor,
    MessageConstructor,
    ParameterConstructor,
    ParameterMessageConstructor,
    MessageParameterConstructor,
    Text,
    Message,
    ParamName,
    InnerException,
    TypeName,
    ActualValue,
    ParameterValueMessageConstructor,
    XmlConstructor,
    XmlLineNumber,
    XmlLinePosition,
    XmlSourceUri,
    XmlResource
}
