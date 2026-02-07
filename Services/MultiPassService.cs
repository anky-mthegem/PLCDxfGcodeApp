using System;
using System.Collections.Generic;
using PLCDxfGcodeApp.Models;

namespace PLCDxfGcodeApp.Services
{
    /// <summary>
    /// Multi-Pass Depth Service
    /// Based on dxfplotter's multi-pass depth cutting algorithm
    /// Generates multiple passes at different Z-levels
    /// </summary>
    public class MultiPassService
    {
        public List<DepthPass> CalculatePasses(double totalDepth, double depthPerPass)
        {
            var passes = new List<DepthPass>();

            if (totalDepth <= 0 || depthPerPass <= 0)
                return passes;

            double currentDepth = 0;
            int passNumber = 1;

            while (currentDepth < totalDepth)
            {
                currentDepth = currentDepth + depthPerPass;
                
                // Don't exceed total depth
                if (currentDepth > totalDepth)
                    currentDepth = totalDepth;

                passes.Add(new DepthPass
                {
                    PassNumber = passNumber,
                    Depth = -currentDepth, // Negative for cutting downward
                    DepthIncrement = depthPerPass
                });

                passNumber = passNumber + 1;
            }

            return passes;
        }

        public List<string> GenerateMultiPassGCode(List<Entity> entities, List<DepthPass> passes, 
            double feedRate, string moveCommand)
        {
            var commands = new List<string>();

            foreach (var pass in passes)
            {
                commands.Add("; Pass " + pass.PassNumber + " - Depth: " + pass.Depth.ToString("F3") + " mm");
                
                foreach (var entity in entities)
                {
                    // Add commands for each entity at this depth
                    foreach (var point in entity.Points)
                    {
                        var cmd = moveCommand
                            .Replace("{X:F3}", point.X.ToString("F3"))
                            .Replace("{Y:F3}", point.Y.ToString("F3"))
                            .Replace("{Z:F3}", pass.Depth.ToString("F3"))
                            .Replace("{F:F3}", feedRate.ToString("F3"));
                        
                        commands.Add(cmd);
                    }
                }

                // Retract after each pass
                commands.Add("G0 Z5 ; Retract");
            }

            return commands;
        }

        public double CalculateEstimatedTime(List<DepthPass> passes, double totalDistance, double feedRate)
        {
            // Estimate in minutes
            // Total distance * number of passes / feed rate
            double totalTime = (totalDistance * passes.Count) / feedRate;
            return totalTime;
        }
    }

    public class DepthPass
    {
        public int PassNumber { get; set; }
        public double Depth { get; set; }
        public double DepthIncrement { get; set; }
    }
}
