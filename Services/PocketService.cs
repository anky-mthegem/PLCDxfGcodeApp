using System;
using System.Collections.Generic;
using System.Linq;
using PLCDxfGcodeApp.Models;

namespace PLCDxfGcodeApp.Services
{
    /// <summary>
    /// Pocket Generation Service with Island Detection
    /// Based on dxfplotter's pocket filling algorithm
    /// </summary>
    public class PocketService
    {
        private PathOffsetService _offsetService;

        public PocketService()
        {
            _offsetService = new PathOffsetService();
        }

        public List<Entity> GeneratePocket(List<Entity> boundaryEntities, double stepover, bool detectIslands)
        {
            var pocketPaths = new List<Entity>();

            if (boundaryEntities == null || boundaryEntities.Count == 0)
                return pocketPaths;

            // Get boundary bounds
            var bounds = CalculateBounds(boundaryEntities);
            if (bounds == null)
                return pocketPaths;

            // Generate concentric offset paths from boundary inward
            var currentEntities = new List<Entity>(boundaryEntities);
            double currentOffset = stepover;

            while (true)
            {
                var offsetEntities = _offsetService.OffsetEntities(currentEntities, currentOffset, true);
                
                // Check if offset still has valid geometry
                if (offsetEntities == null || offsetEntities.Count == 0)
                    break;

                // Check if offset collapsed (area too small)
                if (IsCollapsed(offsetEntities))
                    break;

                pocketPaths.AddRange(offsetEntities);
                currentEntities = offsetEntities;
                currentOffset = stepover;

                // Safety limit to prevent infinite loops
                if (pocketPaths.Count > 1000)
                    break;
            }

            // Detect and handle islands if enabled
            if (detectIslands)
            {
                pocketPaths = RemoveIslandIntersections(pocketPaths, boundaryEntities);
            }

            return pocketPaths;
        }

        private BoundingBox CalculateBounds(List<Entity> entities)
        {
            if (entities == null || entities.Count == 0)
                return null;

            double minX = double.MaxValue;
            double minY = double.MaxValue;
            double maxX = double.MinValue;
            double maxY = double.MinValue;

            foreach (var entity in entities)
            {
                foreach (var point in entity.Points)
                {
                    if (point.X < minX) minX = point.X;
                    if (point.Y < minY) minY = point.Y;
                    if (point.X > maxX) maxX = point.X;
                    if (point.Y > maxY) maxY = point.Y;
                }
            }

            return new BoundingBox
            {
                MinX = minX,
                MinY = minY,
                MaxX = maxX,
                MaxY = maxY,
                Width = maxX - minX,
                Height = maxY - minY
            };
        }

        private bool IsCollapsed(List<Entity> entities)
        {
            if (entities == null || entities.Count == 0)
                return true;

            var bounds = CalculateBounds(entities);
            if (bounds == null)
                return true;

            // Check if area is too small (less than 0.1mm square)
            return bounds.Width < 0.1 || bounds.Height < 0.1;
        }

        private List<Entity> RemoveIslandIntersections(List<Entity> pocketPaths, List<Entity> islands)
        {
            // Simple implementation - in production would use proper polygon clipping
            // This checks basic bounding box intersection
            var filteredPaths = new List<Entity>();

            foreach (var path in pocketPaths)
            {
                bool intersectsIsland = false;

                foreach (var island in islands)
                {
                    if (PathIntersectsIsland(path, island))
                    {
                        intersectsIsland = true;
                        break;
                    }
                }

                if (!intersectsIsland)
                {
                    filteredPaths.Add(path);
                }
            }

            return filteredPaths;
        }

        private bool PathIntersectsIsland(Entity path, Entity island)
        {
            // Simple bounding box check
            var pathBounds = CalculateBounds(new List<Entity> { path });
            var islandBounds = CalculateBounds(new List<Entity> { island });

            if (pathBounds == null || islandBounds == null)
                return false;

            return !(pathBounds.MaxX < islandBounds.MinX ||
                    pathBounds.MinX > islandBounds.MaxX ||
                    pathBounds.MaxY < islandBounds.MinY ||
                    pathBounds.MinY > islandBounds.MaxY);
        }

        public List<Models.Point> GenerateZigzagPattern(BoundingBox bounds, double stepover)
        {
            var points = new List<Models.Point>();
            double y = bounds.MinY;
            bool leftToRight = true;

            while (y <= bounds.MaxY)
            {
                if (leftToRight)
                {
                    points.Add(new Models.Point { X = bounds.MinX, Y = y, Z = 0 });
                    points.Add(new Models.Point { X = bounds.MaxX, Y = y, Z = 0 });
                }
                else
                {
                    points.Add(new Models.Point { X = bounds.MaxX, Y = y, Z = 0 });
                    points.Add(new Models.Point { X = bounds.MinX, Y = y, Z = 0 });
                }

                y = y + stepover;
                leftToRight = !leftToRight;
            }

            return points;
        }
    }

    public class BoundingBox
    {
        public double MinX { get; set; }
        public double MinY { get; set; }
        public double MaxX { get; set; }
        public double MaxY { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
    }
}
