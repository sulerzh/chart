using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Semantic.Reporting.Windows.Common.Internal
{
    public static class GeometryExtensions
    {
        public static IEnumerable<Point> GetPoints(this Geometry geometry)
        {
            if (geometry == null)
                return Enumerable.Empty<Point>();
            RectangleGeometry rectGeometry = geometry as RectangleGeometry;
            if (rectGeometry != null)
                return GeometryExtensions.GetRectPoints(rectGeometry);
            EllipseGeometry ellipseGeometry = geometry as EllipseGeometry;
            if (ellipseGeometry != null)
                return GeometryExtensions.GetEllipsePoints(ellipseGeometry);
            LineGeometry lineGeometry = geometry as LineGeometry;
            if (lineGeometry != null)
                return GeometryExtensions.GetLinePoints(lineGeometry);
            PathGeometry pathGeometry = geometry as PathGeometry;
            if (pathGeometry != null)
                return GeometryExtensions.GetPathPoints(pathGeometry);
            GeometryGroup geometryGroup = geometry as GeometryGroup;
            if (geometryGroup != null)
                return GeometryExtensions.GetGroupPoints(geometryGroup);
            throw new NotSupportedException("This type of geometry is not supported");
        }

        public static IEnumerable<Point> GetRectPoints(this RectangleGeometry rectGeometry)
        {
            if (rectGeometry != null)
            {
                Rect rect = rectGeometry.Rect;
                yield return new Point(rect.Left, rect.Top);
                yield return new Point(rect.Right, rect.Top);
                yield return new Point(rect.Right, rect.Bottom);
                yield return new Point(rect.Left, rect.Bottom);
            }
        }

        public static IEnumerable<Point> GetEllipsePoints(this EllipseGeometry ellipseGeometry)
        {
            if (ellipseGeometry != null)
            {
                yield return new Point(ellipseGeometry.Center.X, ellipseGeometry.Center.Y - ellipseGeometry.RadiusY);
                yield return new Point(ellipseGeometry.Center.X - ellipseGeometry.RadiusX, ellipseGeometry.Center.Y);
                yield return new Point(ellipseGeometry.Center.X, ellipseGeometry.Center.Y + ellipseGeometry.RadiusY);
                yield return new Point(ellipseGeometry.Center.X + ellipseGeometry.RadiusX, ellipseGeometry.Center.Y);
            }
        }

        public static IEnumerable<Point> GetLinePoints(this LineGeometry lineGeometry)
        {
            yield return lineGeometry.StartPoint;
            yield return lineGeometry.EndPoint;
        }

        public static IEnumerable<Point> GetPathPoints(this PathGeometry pathGeometry)
        {
            if (pathGeometry != null)
            {
                foreach (PathFigure pathFigure in pathGeometry.Figures)
                {
                    yield return pathFigure.StartPoint;
                    foreach (PathSegment pathSegment in pathFigure.Segments)
                    {
                        ArcSegment arcSegment = pathSegment as ArcSegment;
                        if (arcSegment != null)
                        {
                            yield return arcSegment.Point;
                        }
                        else
                        {
                            LineSegment lineSergment = pathSegment as LineSegment;
                            if (lineSergment != null)
                            {
                                yield return lineSergment.Point;
                            }
                            else
                            {
                                PolyLineSegment polyLineSegment = pathSegment as PolyLineSegment;
                                if (polyLineSegment != null)
                                {
                                    foreach (Point point in polyLineSegment.Points)
                                        yield return point;
                                }
                                else
                                {
                                    BezierSegment bezierSegment = pathSegment as BezierSegment;
                                    if (bezierSegment != null)
                                    {
                                        yield return bezierSegment.Point3;
                                    }
                                    else
                                    {
                                        PolyBezierSegment polyBezierSegment = pathSegment as PolyBezierSegment;
                                        if (polyBezierSegment != null)
                                        {
                                            foreach (Point point in polyBezierSegment.Points)
                                                yield return point;
                                        }
                                        else
                                        {
                                            QuadraticBezierSegment quadraticBezierSegment = pathSegment as QuadraticBezierSegment;
                                            if (quadraticBezierSegment != null)
                                            {
                                                yield return quadraticBezierSegment.Point2;
                                            }
                                            else
                                            {
                                                PolyQuadraticBezierSegment polyQuadraticBezierSegment = pathSegment as PolyQuadraticBezierSegment;
                                                if (polyQuadraticBezierSegment != null)
                                                {
                                                    foreach (Point point in polyQuadraticBezierSegment.Points)
                                                        yield return point;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public static IEnumerable<Point> GetGroupPoints(this GeometryGroup geometryGroup)
        {
            if (geometryGroup != null)
            {
                foreach (Geometry geometry in geometryGroup.Children)
                {
                    foreach (Point point in GeometryExtensions.GetPoints(geometry))
                        yield return point;
                }
            }
        }

        public static Geometry Clone(this Geometry source)
        {
            if (source == null)
                return (Geometry)null;
            RectangleGeometry source1 = source as RectangleGeometry;
            if (source1 != null)
                return (Geometry)GeometryExtensions.CloneRect(source1);
            EllipseGeometry source2 = source as EllipseGeometry;
            if (source2 != null)
                return (Geometry)GeometryExtensions.CloneEllipse(source2);
            LineGeometry source3 = source as LineGeometry;
            if (source3 != null)
                return (Geometry)GeometryExtensions.CloneLine(source3);
            PathGeometry source4 = source as PathGeometry;
            if (source4 != null)
                return (Geometry)GeometryExtensions.ClonePath(source4);
            GeometryGroup source5 = source as GeometryGroup;
            if (source5 != null)
                return (Geometry)GeometryExtensions.CloneGroup(source5);
            throw new NotSupportedException("This type of geometry is not supported");
        }

        public static RectangleGeometry CloneRect(this RectangleGeometry source)
        {
            if (source == null)
                return (RectangleGeometry)null;
            RectangleGeometry rectangleGeometry = new RectangleGeometry();
            rectangleGeometry.Rect = source.Rect;
            rectangleGeometry.RadiusX = source.RadiusX;
            rectangleGeometry.RadiusY = source.RadiusY;
            rectangleGeometry.Transform = source.Transform;
            return rectangleGeometry;
        }

        public static EllipseGeometry CloneEllipse(this EllipseGeometry source)
        {
            if (source == null)
                return (EllipseGeometry)null;
            EllipseGeometry ellipseGeometry = new EllipseGeometry();
            ellipseGeometry.Center = source.Center;
            ellipseGeometry.RadiusX = source.RadiusX;
            ellipseGeometry.RadiusY = source.RadiusY;
            ellipseGeometry.Transform = source.Transform;
            return ellipseGeometry;
        }

        public static LineGeometry CloneLine(this LineGeometry source)
        {
            if (source == null)
                return (LineGeometry)null;
            LineGeometry lineGeometry = new LineGeometry();
            lineGeometry.StartPoint = source.StartPoint;
            lineGeometry.EndPoint = source.EndPoint;
            lineGeometry.Transform = source.Transform;
            return lineGeometry;
        }

        public static PathGeometry ClonePath(this PathGeometry source)
        {
            if (source == null)
                return (PathGeometry)null;
            PathGeometry pathGeometry = new PathGeometry();
            foreach (PathFigure source1 in source.Figures)
            {
                PathFigure pathFigure = GeometryExtensions.CloneFigure(source1);
                pathGeometry.Figures.Add(pathFigure);
            }
            pathGeometry.FillRule = source.FillRule;
            pathGeometry.Transform = source.Transform;
            return pathGeometry;
        }

        public static PathFigure CloneFigure(PathFigure source)
        {
            if (source == null)
                return (PathFigure)null;
            PathFigure pathFigure = new PathFigure();
            pathFigure.StartPoint = source.StartPoint;
            pathFigure.IsClosed = source.IsClosed;
            pathFigure.IsFilled = source.IsFilled;
            foreach (PathSegment pathSegment in source.Segments)
                pathFigure.Segments.Add(pathSegment.Clone());
            return pathFigure;
        }

        public static PathSegment Clone(this PathSegment source)
        {
            if (source == null)
                return (PathSegment)null;
            ArcSegment source1 = source as ArcSegment;
            if (source1 != null)
                return (PathSegment)GeometryExtensions.CloneArcSegment(source1);
            LineSegment source2 = source as LineSegment;
            if (source2 != null)
                return (PathSegment)GeometryExtensions.CloneLineSegment(source2);
            PolyLineSegment source3 = source as PolyLineSegment;
            if (source3 != null)
                return (PathSegment)GeometryExtensions.ClonePolyLineSegment(source3);
            BezierSegment source4 = source as BezierSegment;
            if (source4 != null)
                return (PathSegment)GeometryExtensions.CloneBezierSegment(source4);
            PolyBezierSegment source5 = source as PolyBezierSegment;
            if (source5 != null)
                return (PathSegment)GeometryExtensions.ClonePolyBezierSegment(source5);
            QuadraticBezierSegment source6 = source as QuadraticBezierSegment;
            if (source6 != null)
                return (PathSegment)GeometryExtensions.CloneQuadraticBezierSegment(source6);
            PolyQuadraticBezierSegment source7 = source as PolyQuadraticBezierSegment;
            if (source7 != null)
                return (PathSegment)GeometryExtensions.ClonePolyQuadraticBezierSegment(source7);
            throw new NotSupportedException("This type of segment is not supported by Clone extension method");
        }

        public static ArcSegment CloneArcSegment(this ArcSegment source)
        {
            if (source == null)
                return (ArcSegment)null;
            return new ArcSegment()
            {
                IsLargeArc = source.IsLargeArc,
                Point = source.Point,
                RotationAngle = source.RotationAngle,
                Size = source.Size,
                SweepDirection = source.SweepDirection
            };
        }

        public static LineSegment CloneLineSegment(this LineSegment source)
        {
            if (source == null)
                return (LineSegment)null;
            return new LineSegment()
            {
                Point = source.Point
            };
        }

        public static PolyLineSegment ClonePolyLineSegment(this PolyLineSegment source)
        {
            if (source == null)
                return (PolyLineSegment)null;
            PolyLineSegment polyLineSegment = new PolyLineSegment();
            foreach (Point point in source.Points)
                polyLineSegment.Points.Add(point);
            return polyLineSegment;
        }

        public static BezierSegment CloneBezierSegment(this BezierSegment source)
        {
            if (source == null)
                return (BezierSegment)null;
            return new BezierSegment()
            {
                Point1 = source.Point1,
                Point2 = source.Point2,
                Point3 = source.Point3
            };
        }

        public static PolyBezierSegment ClonePolyBezierSegment(this PolyBezierSegment source)
        {
            if (source == null)
                return (PolyBezierSegment)null;
            PolyBezierSegment polyBezierSegment = new PolyBezierSegment();
            foreach (Point point in source.Points)
                polyBezierSegment.Points.Add(point);
            return polyBezierSegment;
        }

        public static QuadraticBezierSegment CloneQuadraticBezierSegment(this QuadraticBezierSegment source)
        {
            if (source == null)
                return (QuadraticBezierSegment)null;
            return new QuadraticBezierSegment()
            {
                Point1 = source.Point1,
                Point2 = source.Point2
            };
        }

        public static PolyQuadraticBezierSegment ClonePolyQuadraticBezierSegment(this PolyQuadraticBezierSegment source)
        {
            if (source == null)
                return (PolyQuadraticBezierSegment)null;
            PolyQuadraticBezierSegment quadraticBezierSegment = new PolyQuadraticBezierSegment();
            foreach (Point point in source.Points)
                quadraticBezierSegment.Points.Add(point);
            return quadraticBezierSegment;
        }

        public static GeometryGroup CloneGroup(this GeometryGroup source)
        {
            if (source == null)
                return (GeometryGroup)null;
            GeometryGroup geometryGroup = new GeometryGroup();
            foreach (Geometry source1 in source.Children)
                geometryGroup.Children.Add(GeometryExtensions.Clone(source1));
            geometryGroup.FillRule = source.FillRule;
            geometryGroup.Transform = source.Transform;
            return geometryGroup;
        }
    }
}
