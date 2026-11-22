# Point + Vector WASM

Source: [PointVector.cs](Fixtures/Structs/PointVector.cs)

## Source

```csharp
using nfloat = System.Runtime.InteropServices.NFloat;

public struct Point
{
    public nfloat X;
    public nfloat Y;
    public Point(nfloat x, nfloat y) { X = x; Y = y; }
    public static Point operator +(Point p, Vector v) => new(p.X + v.X, p.Y + v.Y);
}
public struct Vector
{
    public nfloat X;
    public nfloat Y;
    public Vector(nfloat x, nfloat y) { X = x; Y = y; }
}
public static class Program
{
    public static int Main()
    {
        var p = new Point(1, 2);
        var v = new Vector(3, 4);
        var result = p + v;
        System.Console.WriteLine((double)result.X);
        System.Console.WriteLine((double)result.Y);
        return 0;
    }
}
```

## CLR output

```text
4
6
```

## CLR exit code

```text
0
```

## WASM output

```text
Not executed.
```

## Exit Code

```text
Not available.
```

## WAT

```text
Not emitted.
```

## Status

Inconclusive: stopped during WASM emission.

## Exception

```text
System.NotSupportedException: Unsupported Call at IL_0001; only same-module method definitions or explicit rewrites are supported.
```
