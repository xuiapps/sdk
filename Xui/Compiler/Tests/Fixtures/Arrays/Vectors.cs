using System;

public struct Point { public int X, Y; }
public class Scene { public Point[] Points; public string[] Labels; }

public static class Program
{
    private static int Sum(Point[] points)
    {
        int total = 0;
        for (int i = 0; i < points.Length; i++) total += points[i].X + points[i].Y;
        return total;
    }

    public static int Main()
    {
        var scene = new Scene { Points = new Point[2], Labels = new string[2] };
        scene.Points[0].X = 10;
        scene.Points[0].Y = 20;
        scene.Points[1] = scene.Points[0];
        scene.Points[1].X = 5;
        scene.Points[1].Y = 7;
        scene.Labels[0] = "two independent points";
        Console.WriteLine(scene.Labels[0]);
        Console.WriteLine(scene.Points.Length);
        Console.WriteLine(scene.Points[0].X);
        Console.WriteLine(scene.Points[1].X);
        return Sum(scene.Points);
    }
}
