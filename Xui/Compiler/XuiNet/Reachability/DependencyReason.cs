namespace Xui.Compiler.XuiNet.Reachability;

/// <summary>Why the source requirement retains its target.</summary>
public enum DependencyReason
{
    Root, DeclaringType, Signature, Local, BaseType, Interface, GenericArgument,
    Layout, InstanceField, FieldType, FieldRead, FieldWrite, FieldAddress,
    Call, VirtualCall, VirtualImplementation, Allocation, TypeOperand,
    Initialization, Initializer, Literal, Rewrite
}
