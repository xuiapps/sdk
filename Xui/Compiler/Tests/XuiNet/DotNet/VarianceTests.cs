using Microsoft.CodeAnalysis;
using Xui.Compiler.XuiNet;
using Xui.Compiler.XuiNet.DotNet;
using Xunit;

namespace Xui.Compiler.Tests.XuiNet.DotNet;

public class VarianceTests
{
    [Theory]
    [InlineData("string", "System.IComparable<string>", true)]
    [InlineData("string", "ICustom", false)]
    [InlineData("int[]", "System.Array", true)]
    [InlineData("Dog[]", "Animal[]", true)]
    [InlineData("int[]", "object[]", false)]
    [InlineData("Dog", "Animal", true)]
    [InlineData("Animal", "Dog", false)]
    [InlineData("int", "object", false)]
    public void CovariantArgumentsFollowReferenceConversions(string source, string target, bool expected)
    {
        using var assembly = Read(source, target);
        using var framework = new Frontend.Framework();
        _ = new Xui.Compiler.XuiNet.Compilation([assembly, .. framework.Assemblies]);
        Assert.Equal(expected, Contract(assembly, "Source").IsAssignableTo(Contract(assembly, "Target")));
    }

    [Fact]
    public void SynthesizedArrayInterfacesFailExplicitly()
    {
        using var assembly = Read("int[]", "System.Collections.Generic.IEnumerable<int>");
        using var framework = new Frontend.Framework();
        _ = new Xui.Compiler.XuiNet.Compilation([assembly, .. framework.Assemblies]);
        Assert.Contains("Array interface variance", Assert.Throws<NotSupportedException>(() =>
            Contract(assembly, "Source").IsAssignableTo(Contract(assembly, "Target"))).Message);
    }

    private static Assembly Read(string source, string target)
    {
        var pe = Frontend.Roslyn.Compile($$"""
            public interface ICustom { }
            public class Animal { } public class Dog : Animal { }
            public interface ISource<out T> { }
            public class Source : ISource<{{source}}> { }
            public class Target : ISource<{{target}}> { }
            """, OutputKind.DynamicallyLinkedLibrary);
        using var stream = new MemoryStream(pe);
        return AssemblyCompiler.Read(stream);
    }

    private static TypeIdentity Contract(Assembly assembly, string name)
    {
        var module = assembly.ManifestModule;
        var type = new TypeIdentity(module, module.Types.Single(handle => module.GetType(handle).Name == name));
        return Assert.Single(type.Interfaces());
    }
}
