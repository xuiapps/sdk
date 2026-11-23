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
