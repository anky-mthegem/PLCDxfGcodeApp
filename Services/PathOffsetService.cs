using System;
using System.Collections.Generic;
using PLCDxfGcodeApp.Models;

namespace PLCDxfGcodeApp.Services
{
    /// <summary>
    /// Path Offsetting Service for Tool Compensation
    /// Based on dxfplotter's path offsetting algorithm
    /// </summary>
    public class PathOffsetService
    {
        public List<Entity> OffsetEntities(List<Entity> entities, double offsetAmount, bool inward)
        {
            var offsetEntities = new List<Entity>();
            double direction = inward ? -1.0 : 1.0;
            double offset = offsetAmount * direction;

            foreach (var entity in entities)
            {
                if (entity.Type == "Line")
                {
                    offsetEntities.Add(OffsetLine(entity, offset));
                }
                else if (entity.Type == "Circle")
                {
                    offsetEntities.Add(OffsetCircle(entity, offset));
                }
                else if (entity.Type == "LwPolyline")
                {
                    offsetEntities.Add(OffsetPolyline(entity, offset));
                }
                else
                {
                    offsetEntities.Add(entity); // Keep unchanged if unsupported
                }
            }

            return offsetEntities;
        }

        private Entity OffsetLine(Entity line, double offset)
        {
            if (line.Points.Count < 2)
                return line;

            var p1 = line.Points[0];
            var p2 = line.Points[1];

            // Calculate perpendicular vector
            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;
            double length = Math.Sqrt(dx * dx + dy * dy);

            if (length < 0.0001)
                return line;

            // Normalize and rotate 90 degrees
            double nx = -dy / length;
            double ny = dx / length;

            var offsetLine = new Entity
            {
                Type = "Line",
                Points = new List<Models.Point>
                {
                    new Models.Point { X = p1.X + nx * offset, Y = p1.Y + ny * offset, Z = p1.Z },
                    new Models.Point { X = p2.X + nx * offset, Y = p2.Y + ny * offset, Z = p2.Z }
                }
            };

            return offsetLine;
        }

        private Entity OffsetCircle(Entity circle, double offset)
        {
            if (circle.Points.Count < 1)
                return circle;

            var center = circle.Points[0];
            
            var offsetCircle = new Entity
            {
                Type = "Circle",
                Points = new List<Models.Point>
                {
                    new Models.Point { X = center.X, Y = center.Y, Z = center.Z }
                },
                Radius = circle.Radius + offset
            };

            return offsetCircle;
        }

        private Entity OffsetPolyline(Entity polyline, double offset)
        {
            if (polyline.Points.Count < 2)
                return polyline;

            var offsetPoints = new List<Models.Point>();

            for (int i = 0; i < polyline.Points.Count - 1; i++)
            {
                var p1 = polyline.Points[i];
                var p2 = polyline.Points[i + 1];

                // Calculate perpendicular offset
                double dx = p2.X - p1.X;
                double dy = p2.Y - p1.Y;
                double length = Math.Sqrt(dx * dx + dy * dy);

                if (length < 0.0001)
                    continue;

                double nx = -dy / length;
                double ny = dx / length;

                offsetPoints.Add(new Models.Point
                {
                    X = p1.X + nx * offset,
                    Y = p1.Y + ny * offset,
                    Z = p1.Z
                });
            }

            // Add last point
            if (polyline.Points.Count > 0)
            {
                var lastIdx = polyline.Points.Count - 1;
                var prevIdx = lastIdx - 1;
                
                if (prevIdx >= 0)
                {
                    var p1 = polyline.Points[prevIdx];
                    var p2 = polyline.Points[lastIdx];

                    double dx = p2.X - p1.X;
                    double dy = p2.Y - p1.Y;
                    double length = Math.Sqrt(dx * dx + dy * dy);

                    if (length >= 0.0001)
                    {
                        double nx = -dy / length;
                        double ny = dx / length;

                        offsetPoints.Add(new Models.Point
                        {
                            X = p2.X + nx * offset,
                            Y = p2.Y + ny * offset,
                            Z = p2.Z
                        });
                    }
                }
            }

            return new Entity
            {
                Type = "LwPolyline",
                Points = offsetPoints
            };
        }
    }
}
