using System;
using System.Runtime.InteropServices;

[Flags] public enum Options : byte { None, Read = 1, Write = 128 }
public enum Delta : sbyte { Back = -7 }
public enum Wide : ulong { High = 0x8000000000000001UL }

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Packet
{
    public byte Prefix;
    public Options Options;
    public Delta Delta;
    public Wide Wide;
    public byte Suffix;
}

public class Box { public Packet Value; }

public static class Program
{
    static Packet Copy(Packet packet) => packet;
    static Options Choose(bool write) => write ? Options.Read | Options.Write : Options.Read;

    public static int Main()
    {
        var packet = new Packet();
        packet.Prefix = 17;
        packet.Options = Choose(true);
        packet.Delta = Delta.Back;
        packet.Wide = Wide.High;
        packet.Suffix = 29;
        var box = new Box();
        box.Value = Copy(packet);
        var copy = box.Value;
        var values = new Options[2];
        values[1] = copy.Options;
        Console.WriteLine(copy.Prefix);
        Console.WriteLine((int)values[1]);
        Console.WriteLine((int)copy.Delta);
        Console.WriteLine(copy.Wide == Wide.High);
        Console.WriteLine(copy.Wide > (Wide)long.MaxValue);
        Console.WriteLine(copy.Suffix);
        return (values[1] & Options.Write) != 0 ? 42 : 1;
    }
}
