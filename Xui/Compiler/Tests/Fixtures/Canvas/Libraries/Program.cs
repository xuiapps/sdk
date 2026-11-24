public struct Local { public int Value; }
public static class Program
{
    static void Move(ref Coordinates.Point point) { point.X += 2; }
    public static int Main()
    {
        Local local = default; local.Value = 3;
        Geometry.Box box = default;
        box.Origin.X = 10; box.Origin.Y = 20; box.Width = 7;
        Geometry.Box copy = box;
        Move(ref copy.Origin);
        box.Origin.X = 99;
        return copy.Origin.X + copy.Origin.Y + copy.Width + local.Value;
    }
}
