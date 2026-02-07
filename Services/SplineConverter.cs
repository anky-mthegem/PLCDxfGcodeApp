using System;
using System.Collections.Generic;
using PLCDxfGcodeApp.Models;

namespace PLCDxfGcodeApp.Services
{
    /// <summary>
    /// Spline to Arc Converter
    /// Based on dxfplotter's spline to biarc conversion algorithm
    /// Converts complex spline curves to arc segments for better CNC compatibility
    /// </summary>
    public class SplineConverter
    {
        private double _tolerance;

        public SplineConverter(double tolerance = 0.01)
        {
            _tolerance = tolerance;
        }

        public List<Entity> ConvertSplineToArcs(Entity splineEntity)
        {
            var arcEntities = new List<Entity>();

            if (splineEntity == null || splineEntity.Points.Count < 3)
                return arcEntities;

            // Convert spline points to arc segments
            for (int i = 0; i < splineEntity.Points.Count - 2; i++)
            {
                var p1 = splineEntity.Points[i];
                var p2 = splineEntity.Points[i + 1];
                var p3 = splineEntity.Points[i + 2];

                var arc = FitArcThroughPoints(p1, p2, p3);
                if (arc != null)
                {
                    arcEntities.Add(arc);
                }
            }

            return arcEntities;
        }

        private Entity FitArcThroughPoints(Models.Point p1, Models.Point p2, Models.Point p3)
        {
            // Calculate arc center and radius through three points
            double ax = p1.X;
            double ay = p1.Y;
            double bx = p2.X;
            double by = p2.Y;
            double cx = p3.X;
            double cy = p3.Y;

            // Calculate perpendicular bisectors
            double d = 2 * (ax * (by - cy) + bx * (cy - ay) + cx * (ay - by));
            
            if (Math.Abs(d) < 0.0001)
            {
                // Points are collinear, return line instead
                return new Entity
                {
                    Type = "Line",
                    Points = new List<Models.Point> { p1, p2, p3 }
                };
            }

            // Calculate center
            double ux = ((ax * ax + ay * ay) * (by - cy) + (bx * bx + by * by) * (cy - ay) + (cx * cx + cy * cy) * (ay - by)) / d;
            double uy = ((ax * ax + ay * ay) * (cx - bx) + (bx * bx + by * by) * (ax - cx) + (cx * cx + cy * cy) * (bx - ax)) / d;

            // Calculate radius
            double radius = Math.Sqrt((ax - ux) * (ax - ux) + (ay - uy) * (ay - uy));

            // Calculate angles
            double startAngle = Math.Atan2(ay - uy, ax - ux);
            double endAngle = Math.Atan2(cy - uy, cx - ux);

            return new Entity
            {
                Type = "Arc",
                Points = new List<Models.Point>
                {
                    new Models.Point { X = ux, Y = uy, Z = p1.Z }, // Center
                    new Models.Point { X = ax, Y = ay, Z = p1.Z }, // Start
                    new Models.Point { X = cx, Y = cy, Z = p3.Z }  // End
                },
                Radius = radius,
                StartAngle = startAngle * 180 / Math.PI,
                EndAngle = endAngle * 180 / Math.PI
            };
        }

        public List<Entity> ApproximateSplineWithLines(Entity splineEntity, double segmentLength)
        {
            var lineEntities = new List<Entity>();

            if (splineEntity == null || splineEntity.Points.Count < 2)
                return lineEntities;

            // Simple approximation: break spline into small line segments
            for (int i = 0; i < splineEntity.Points.Count - 1; i++)
            {
                var p1 = splineEntity.Points[i];
                var p2 = splineEntity.Points[i + 1];

                double distance = Math.Sqrt(
                    (p2.X - p1.X) * (p2.X - p1.X) +
                    (p2.Y - p1.Y) * (p2.Y - p1.Y)
                );

                int segments = (int)Math.Ceiling(distance / segmentLength);
                
                for (int j = 0; j < segments; j++)
                {
                    double t1 = (double)j / segments;
                    double t2 = (double)(j + 1) / segments;

                    var sp1 = InterpolatePoint(p1, p2, t1);
                    var sp2 = InterpolatePoint(p1, p2, t2);

                    lineEntities.Add(new Entity
                    {
                        Type = "Line",
                        Points = new List<Models.Point> { sp1, sp2 }
                    });
                }
            }

            return lineEntities;
        }

        private Models.Point InterpolatePoint(Models.Point p1, Models.Point p2, double t)
        {
            return new Models.Point
            {
                X = p1.X + (p2.X - p1.X) * t,
                Y = p1.Y + (p2.Y - p1.Y) * t,
                Z = p1.Z + (p2.Z - p1.Z) * t
            };
        }

        public bool IsArcWithinTolerance(Entity arc, List<Models.Point> originalPoints)
        {
            if (arc == null || originalPoints == null || originalPoints.Count == 0)
                return false;

            // Check if all original points are within tolerance of the arc
            var center = arc.Points[0];
            double radius = arc.Radius;

            foreach (var point in originalPoints)
            {
                double distance = Math.Sqrt(
                    (point.X - center.X) * (point.X - center.X) +
                    (point.Y - center.Y) * (point.Y - center.Y)
                );

                if (Math.Abs(distance - radius) > _tolerance)
                    return false;
            }

            return true;
        }
    }
}
