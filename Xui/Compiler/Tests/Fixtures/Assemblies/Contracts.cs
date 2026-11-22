using System.Reflection;

[assembly: AssemblyVersion("1.2.3.4")]

namespace Samples.Contracts;

public interface IGreeter
{
    string Greet();
}

public readonly struct Greeting
{
    public readonly string Text;

    public Greeting(string text) => Text = text;
}

public sealed class Greeter : IGreeter
{
    private readonly Greeting greeting;

    public Greeter(string text) => greeting = new Greeting(text);

    public string Greet() => greeting.Text;

    public sealed class Options
    {
        public string Prefix = "Hello | Здравей\nWorld";
    }
}
