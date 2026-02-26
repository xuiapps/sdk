using System.Runtime.InteropServices;
using Xui.Core.Canvas;
using Xui.Core.Curves2D;
using Xui.Core.Math2D;
using static Xui.Runtime.Windows.D2D1;

namespace Xui.Runtime.Windows.Actual;

public partial class Direct2DContext
{
    private struct PathStr
    {
        // Position epsilon (DIPs). If you see occasional “almost equal” points, bump slightly (e.g. 1e-3f).
        private const float PosEps = 1e-8f;
        private const float PosEpsSq = PosEps * PosEps;

        // Minimum dimension used when clamping a degenerate (zero) rect side.
        // Must be > PosEps so the four corners are not collapsed by IsSamePoint,
        // but small enough to be invisible (<< 1 DIP).
        private const float MinRectDim = 1e-4f;

        // Radius epsilon
        private const float RadiusEps = 1e-8f;

        public static readonly float HalfPI = float.Pi / 2f;

        public readonly Factory3 Factory;

        public PathGeometry.Ptr PathGeometry;

        public GeometrySink.Ptr GeometrySink;

        private Point point = Point.Zero;
        private bool hasOpenFigure;

        public PathStr(Factory3 factory)
        {
            this.Factory = factory;
        }

        public void Dispose()
        {
            this.GeometrySink.Dispose();
            this.PathGeometry.Dispose();
        }

        public void BeginPath()
        {
            this.CloseFigureIfAny();
            if (!this.GeometrySink.IsNull) this.GeometrySink.Close();
            this.ClearAfterUse();
        }

        public void CurveTo(Point cp1, Point to)
        {
            this.CreatePathOnDemand();
            this.BeginFigureOnDemand();

            // Degenerate: all three points coincide — control point adds nothing
            if (IsSamePoint(this.point, cp1) && IsSamePoint(cp1, to))
            {
                this.point = to;
                return;
            }

            this.GeometrySink.AddQuadraticBezier(new QuadraticBezierSegment()
            {
                Point1 = cp1,
                Point2 = to
            });

            this.point = to;
        }

        public void CurveTo(Point cp1, Point cp2, Point to)
        {
            this.CreatePathOnDemand();
            this.BeginFigureOnDemand();

            // Degenerate: all four points coincide — both control points add nothing
            if (IsSamePoint(this.point, cp1) && IsSamePoint(cp1, cp2) && IsSamePoint(cp2, to))
            {
                this.point = to;
                return;
            }

            this.GeometrySink.AddBezier(new BezierSegment()
            {
                Point1 = cp1,
                Point2 = cp2,
                Point3 = to
            });

            this.point = to;
        }

        public void LineTo(Point to)
        {
            this.CreatePathOnDemand();
            this.BeginFigureOnDemand();

            if (!IsSamePoint(this.point, to))
                this.GeometrySink.AddLine(to);

            this.point = to;
        }

        public void MoveTo(Point to)
        {
            this.CloseFigureIfAny();
            this.point = to;
        }

        public void ClosePath()
        {
            if (this.hasOpenFigure)
            {
                this.hasOpenFigure = false;
                this.GeometrySink.EndFigure(FigureEnd.Closed);
            }
        }

        public void CreatePathOnDemand()
        {
            if (this.PathGeometry.IsNull)
            {
                this.PathGeometry = this.Factory.CreatePathGeometryPtr();
                this.GeometrySink = this.PathGeometry.Open();
            }
        }

        public void BeginFigureOnDemand()
        {
            if (!this.hasOpenFigure)
            {
                this.GeometrySink.BeginFigure(this.point, FigureBegin.Filled);
                this.hasOpenFigure = true;
            }
        }

        public void BeginFigureOnDemandOrLineTo(Point startPoint)
        {
            if (this.hasOpenFigure)
            {
                this.LineTo(startPoint);
            }
            else
            {
                this.MoveTo(startPoint);
                this.GeometrySink.BeginFigure(this.point, FigureBegin.Filled);
                this.hasOpenFigure = true;
            }
        }

        public void CloseFigureIfAny()
        {
            if (this.hasOpenFigure)
            {
                this.hasOpenFigure = false;
                this.GeometrySink.EndFigure(FigureEnd.Open);
            }
        }

        public PathGeometry.Ptr PrepareToUse()
        {
            this.CloseFigureIfAny();
            if (!this.GeometrySink.IsNull) this.GeometrySink.Close();
            return this.PathGeometry;
        }

        public void SetFillMode(FillMode fillMode)
        {
            if (!this.GeometrySink.IsNull)
                this.GeometrySink.SetFillMode(fillMode);
        }

        public void ClearAfterUse()
        {
            this.GeometrySink.Dispose();
            this.PathGeometry.Dispose();
        }

        public void Arc(Point center, NFloat radius, NFloat startAngle, NFloat endAngle, Winding winding)
        {
            var arc = new Xui.Core.Curves2D.Arc(center, radius, radius, 0, startAngle, endAngle, winding);
            this.AddArc(arc);
        }

        public void ArcTo(Point cp1, Point cp2, NFloat radius)
        {
            this.CreatePathOnDemand();
            this.BeginFigureOnDemand();

            // Degenerate radius
            if ((float)NFloat.Abs(radius) <= RadiusEps)
            {
                this.LineTo(cp1);
                this.point = cp1;
                return;
            }

            // Degenerate points
            if (IsSamePoint(this.point, cp1) || IsSamePoint(cp1, cp2))
            {
                this.LineTo(cp1);
                this.point = cp1;
                return;
            }

            Vector arm1 = this.point - cp1; // cp1 -> current
            Vector arm2 = cp2 - cp1;        // cp1 -> cp2

            NFloat len1 = arm1.Length;
            NFloat len2 = arm2.Length;

            if ((float)len1 <= PosEps || (float)len2 <= PosEps)
            {
                this.LineTo(cp1);
                this.point = cp1;
                return;
            }

            Vector v1 = arm1 / len1;
            Vector v2 = arm2 / len2;

            NFloat cross = Vector.Cross(v1, v2);
            NFloat dot = Vector.Dot(v1, v2);

            // Collinear / nearly collinear => no arc
            if (NFloat.Abs(cross) < 1e-6f)
            {
                this.LineTo(cp1);
                this.point = cp1;
                return;
            }

            // U-turn: dot ~ -1 => treat as line
            if (dot < -0.9999f)
            {
                this.LineTo(cp1);
                this.point = cp1;
                return;
            }

            NFloat halfAngle = NFloat.Atan2(NFloat.Abs(cross), dot) / 2;
            NFloat tanHalf = NFloat.Tan(halfAngle);

            if (NFloat.Abs(tanHalf) < 1e-6f)
            {
                this.LineTo(cp1);
                this.point = cp1;
                return;
            }

            // *** KEY FIX: clamp radius to max feasible radius for this corner ***
            NFloat maxRadius = NFloat.Min(len1, len2) * tanHalf;
            if (radius > maxRadius) radius = maxRadius;

            if ((float)NFloat.Abs(radius) <= RadiusEps)
            {
                this.LineTo(cp1);
                this.point = cp1;
                return;
            }

            NFloat tangentDist = radius / tanHalf;
            if (NFloat.IsNaN(tangentDist) || NFloat.IsInfinity(tangentDist))
            {
                this.LineTo(cp1);
                this.point = cp1;
                return;
            }

            Point startPoint = cp1 + v1 * tangentDist;
            Point endPoint = cp1 + v2 * tangentDist;

            this.LineTo(startPoint);

            // If endpoint is current, skip arc (degenerate)
            if (!IsSamePoint(this.point, endPoint))
            {
                this.GeometrySink.AddArc(new ArcSegment
                {
                    Point = endPoint,
                    Size = new SizeF((float)radius),
                    RotationAngle = 0f,
                    SweepDirection = cross < 0 ? SweepDirection.Clockwise : SweepDirection.CounterClockwise,
                    ArcSize = ArcSize.Small
                });
            }

            this.point = endPoint;
        }

        public void Ellipse(Point center, NFloat radiusX, NFloat radiusY, NFloat rotation, NFloat startAngle, NFloat endAngle, Winding winding)
        {
            var arc = new Xui.Core.Curves2D.Arc(center, radiusX, radiusY, rotation, startAngle, endAngle, winding);
            this.AddArc(arc);
        }

        private void AddArc(Xui.Core.Curves2D.Arc arc)
        {
            // Degenerate: radius so small the arc is a single point — just line to it
            if ((float)NFloat.Max(NFloat.Abs(arc.RadiusX), NFloat.Abs(arc.RadiusY)) <= RadiusEps)
            {
                this.CreatePathOnDemand();
                this.BeginFigureOnDemand();
                var pt = arc.Evaluate(0);
                this.LineTo(pt);
                return;
            }

            var (ep1, ep2) = arc.ToEndpointArcs();

            this.CreatePathOnDemand();
            this.BeginFigureOnDemandOrLineTo(ep1.Start);

            // Only emit arc if start != end (degenerate arc would confuse D2D)
            if (!IsSamePoint(ep1.Start, ep1.End))
            {
                this.GeometrySink.AddArc(ToArcSegment(ep1));
            }
            this.point = ep1.End;

            if (ep2 != null)
            {
                if (!IsSamePoint(ep2.Value.Start, ep2.Value.End))
                {
                    this.GeometrySink.AddArc(ToArcSegment(ep2.Value));
                }
                this.point = ep2.Value.End;
            }
        }

        private static ArcSegment ToArcSegment(ArcEndpoint ep)
        {
            return new ArcSegment
            {
                Point = ep.End,
                Size = new SizeF((float)ep.RadiusX, (float)ep.RadiusY),
                RotationAngle = float.RadiansToDegrees((float)ep.Rotation),
                SweepDirection = ep.Winding == Winding.ClockWise ? SweepDirection.Clockwise : SweepDirection.CounterClockwise,
                ArcSize = ep.LargeArc ? ArcSize.Large : ArcSize.Small
            };
        }

        public void Rect(Rect rect)
        {
            float w = (float)rect.Width;
            float h = (float)rect.Height;

            // 0×0 → just a point
            if (w * w <= PosEpsSq && h * h <= PosEpsSq)
            {
                this.MoveTo(rect.TopLeft);
                return;
            }

            // Clamp degenerate dimensions to a sub-pixel epsilon so D2D gets a valid closed shape
            if (h * h <= MinRectDim) rect = new Rect(rect.X, rect.Y, rect.Width, MinRectDim);
            if (w * w <= MinRectDim) rect = new Rect(rect.X, rect.Y, MinRectDim, rect.Height);

            this.CreatePathOnDemand();
            this.BeginFigureOnDemand();
            this.MoveTo(rect.TopLeft);
            this.LineTo(rect.TopRight);
            this.LineTo(rect.BottomRight);
            this.LineTo(rect.BottomLeft);
            this.ClosePath();
        }

        public void RoundRect(Rect rect, NFloat radius) =>
            this.RoundRect(rect, new CornerRadius(radius));

        public void RoundRect(Rect rect, CornerRadius radius)
        {
            // All radii zero → degenerate to plain Rect
            if ((float)radius.TopLeft <= RadiusEps && (float)radius.TopRight <= RadiusEps &&
                (float)radius.BottomRight <= RadiusEps && (float)radius.BottomLeft <= RadiusEps)
            {
                this.Rect(rect);
                return;
            }

            this.CreatePathOnDemand();
            this.BeginFigureOnDemandOrLineTo(rect.TopLeft + (0, radius.TopLeft));

            this.AddCornerArc(rect.TopLeft + (radius.TopLeft, 0), (float)radius.TopLeft);
            this.LineTo(rect.TopRight + (-radius.TopRight, 0));
            this.AddCornerArc(rect.TopRight + (0, radius.TopRight), (float)radius.TopRight);
            this.LineTo(rect.BottomRight + (0, -radius.BottomRight));
            this.AddCornerArc(rect.BottomRight + (-radius.BottomRight, 0), (float)radius.BottomRight);
            this.LineTo(rect.BottomLeft + (radius.BottomLeft, 0));
            this.AddCornerArc(rect.BottomLeft + (0, -radius.BottomLeft), (float)radius.BottomLeft);
            this.ClosePath();
        }

        // Adds a 90° clockwise corner arc to the sink, skipping if the radius is degenerate.
        private void AddCornerArc(Point to, float radius)
        {
            if (radius <= RadiusEps || IsSamePoint(this.point, to))
            {
                this.point = to;
                return;
            }

            this.GeometrySink.AddArc(new ArcSegment
            {
                Point = to,
                Size = new SizeF(radius),
                RotationAngle = HalfPI,
                SweepDirection = SweepDirection.Clockwise,
                ArcSize = ArcSize.Small
            });
            this.point = to;
        }

        private static bool IsSamePoint(Point a, Point b)
        {
            float dx = (float)(a.X - b.X);
            float dy = (float)(a.Y - b.Y);
            return (dx * dx + dy * dy) <= PosEpsSq;
        }
    }
}
