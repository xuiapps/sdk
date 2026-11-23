using System.Reflection;

[assembly: AssemblyVersion("3.0.1.0")]

namespace Samples.Inheritance;

public interface IValue
{
    int GetValue();
}

public abstract class ValueSource : IValue
{
    public abstract int GetValue();

    public virtual string Describe() => "value";
}

public readonly struct Result
{
    public readonly int Value;

    public Result(int value) => Value = value;
}

public sealed class FixedValue : ValueSource
{
    private readonly int value;

    public FixedValue(int value) => this.value = value;

    public override int GetValue() => value;

    public override string Describe() => "fixed";

    public static Result Create(int value) => new Result(value);

    public static void Copy(ref Result source, out Result destination) => destination = source;
}
