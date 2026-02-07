using System;
using System.Collections.Generic;
using System.IO;
using PLCDxfGcodeApp.Models;

namespace PLCDxfGcodeApp.Services
{
    public class GcodeService
    {
        private GcodeModel _currentGcode;
        private PathOffsetService _offsetService;
        private PocketService _pocketService;
        private MultiPassService _multiPassService;
        private SplineConverter _splineConverter;
        private GcodeTemplateService _templateService;

        public GcodeService()
        {
            _offsetService = new PathOffsetService();
            _pocketService = new PocketService();
            _multiPassService = new MultiPassService();
            _splineConverter = new SplineConverter();
            _templateService = new GcodeTemplateService();
        }

        public string GenerateGCode(DxfModel dxfModel, double feedRate, int spindleSpeed, double toolDiameter)
        {
            return GenerateGCodeAdvanced(dxfModel, new GcodeModel
            {
                FeedRate = feedRate,
                SpindleSpeed = spindleSpeed,
                ToolDiameter = toolDiameter,
                EnablePathOffset = false,
                EnablePocketGeneration = false,
                EnableMultiPass = false,
                ConvertSplinesToArcs = false,
                Template = new GcodeTemplate()
            });
        }

        public string GenerateGCodeAdvanced(DxfModel dxfModel, GcodeModel settings)
        {
            if (dxfModel == null || dxfModel.Entities.Count == 0)
                throw new InvalidOperationException("No DXF entities to convert");

            var entities = new List<Entity>(dxfModel.Entities);

            // Step 1: Convert splines to arcs if enabled
            if (settings.ConvertSplinesToArcs)
            {
                entities = ConvertSplines(entities, settings.ArcTolerance);
            }

            // Step 2: Apply path offsetting if enabled
            if (settings.EnablePathOffset)
            {
                entities = _offsetService.OffsetEntities(entities, settings.PathOffsetAmount, settings.OffsetInward);
            }

            // Step 3: Generate pocket if enabled
            if (settings.EnablePocketGeneration)
            {
                var pocketPaths = _pocketService.GeneratePocket(entities, settings.PocketStepover, settings.DetectIslands);
                entities.AddRange(pocketPaths);
            }

            var commands = new List<string>();

            // G-Code header
            commands.Add("; Generated G-Code from DXF with Advanced Features");
            commands.Add("; Feed Rate: " + settings.FeedRate + " mm/min");
            commands.Add("; Spindle Speed: " + settings.SpindleSpeed + " RPM");
            commands.Add("; Tool Diameter: " + settings.ToolDiameter + " mm");
            
            if (settings.EnablePathOffset)
                commands.Add("; Path Offset: " + settings.PathOffsetAmount + " mm (" + (settings.OffsetInward ? "Inward" : "Outward") + ")");
            
            if (settings.EnablePocketGeneration)
                commands.Add("; Pocket Generation: Enabled (Stepover: " + settings.PocketStepover + " mm)");
            
            if (settings.EnableMultiPass)
                commands.Add("; Multi-Pass: Total Depth " + settings.TotalDepth + " mm, Depth/Pass " + settings.DepthPerPass + " mm");
            
            commands.Add("; ");
            commands.Add("G21 ; Metric");
            commands.Add("G90 ; Absolute positioning");
            
            // Spindle control using template
            if (settings.Template != null && !string.IsNullOrEmpty(settings.Template.PreCut))
            {
                commands.Add(_templateService.GenerateSpindleControl(settings.SpindleSpeed, 
                    settings.Template.PreCut, settings.Template.PostCut, true));
            }
            else
            {
                commands.Add("S" + settings.SpindleSpeed + " ; Set spindle speed");
                commands.Add("M3 ; Spindle on");
            }
            
            commands.Add("F" + settings.FeedRate + " ; Set feed rate");
            commands.Add("G0 Z10 ; Move to safe height");

            // Step 4: Generate multi-pass if enabled
            if (settings.EnableMultiPass)
            {
                var passes = _multiPassService.CalculatePasses(settings.TotalDepth, settings.DepthPerPass);
                commands.AddRange(GenerateMultiPassCommands(entities, passes, settings));
            }
            else
            {
                // Convert entities to G-Code
                commands.AddRange(GenerateSinglePassCommands(entities, settings));
            }

            // G-Code footer
            commands.Add("G0 Z10 ; Move to safe height");
            
            if (settings.Template != null && !string.IsNullOrEmpty(settings.Template.PostCut))
            {
                commands.Add(_templateService.GenerateSpindleControl(0, 
                    settings.Template.PreCut, settings.Template.PostCut, false));
            }
            else
            {
                commands.Add("M5 ; Spindle off");
            }
            
            commands.Add("M30 ; Program end");

            var gcode = string.Join("\n", commands);
            _currentGcode = settings;
            _currentGcode.Code = gcode;
            _currentGcode.GeneratedDateTime = DateTime.Now;

            return gcode;
        }

        private List<Entity> ConvertSplines(List<Entity> entities, double tolerance)
        {
            var converted = new List<Entity>();

            foreach (var entity in entities)
            {
                if (entity.Type == "Spline")
                {
                    var arcs = _splineConverter.ConvertSplineToArcs(entity);
                    converted.AddRange(arcs);
                }
                else
                {
                    converted.Add(entity);
                }
            }

            return converted;
        }

        private List<string> GenerateSinglePassCommands(List<Entity> entities, GcodeModel settings)
        {
            var commands = new List<string>();
            bool isFirstPoint = true;

            foreach (var entity in entities)
            {
                if (entity.Points.Count < 1) continue;

                if (entity.Type == "Line")
                {
                    commands.AddRange(GenerateLineCommands(entity, isFirstPoint, settings));
                    isFirstPoint = false;
                }
                else if (entity.Type == "Circle")
                {
                    commands.AddRange(GenerateCircleCommands(entity, settings.ToolDiameter, settings));
                }
                else if (entity.Type == "Arc")
                {
                    commands.AddRange(GenerateArcCommands(entity, settings));
                }
                else if (entity.Type == "LwPolyline")
                {
                    commands.AddRange(GeneratePolylineCommands(entity, isFirstPoint, settings));
                    isFirstPoint = false;
                }
            }

            return commands;
        }

        private List<string> GenerateMultiPassCommands(List<Entity> entities, List<DepthPass> passes, GcodeModel settings)
        {
            var commands = new List<string>();

            foreach (var pass in passes)
            {
                commands.Add("");
                commands.Add("; ====== Pass " + pass.PassNumber + " - Depth: " + pass.Depth.ToString("F3") + " mm ======");
                
                bool isFirstPoint = true;
                foreach (var entity in entities)
                {
                    if (entity.Points.Count < 1) continue;

                    if (entity.Type == "Line")
                    {
                        commands.AddRange(GenerateLineCommandsWithDepth(entity, isFirstPoint, pass.Depth, settings));
                        isFirstPoint = false;
                    }
                    else if (entity.Type == "Circle")
                    {
                        commands.AddRange(GenerateCircleCommandsWithDepth(entity, pass.Depth, settings));
                    }
                    else if (entity.Type == "Arc")
                    {
                        commands.AddRange(GenerateArcCommandsWithDepth(entity, pass.Depth, settings));
                    }
                    else if (entity.Type == "LwPolyline")
                    {
                        commands.AddRange(GeneratePolylineCommandsWithDepth(entity, isFirstPoint, pass.Depth, settings));
                        isFirstPoint = false;
                    }
                }

                // Retract after each pass
                commands.Add("G0 Z5 ; Retract after pass " + pass.PassNumber);
            }

            return commands;
        }

        private List<string> GenerateLineCommands(Entity entity, bool isFirstPoint, GcodeModel settings)
        {
            var commands = new List<string>();

            for (int i = 0; i < entity.Points.Count; i++)
            {
                var point = entity.Points[i];

                if (isFirstPoint && i == 0)
                {
                    if (settings.Template != null && !string.IsNullOrEmpty(settings.Template.PlaneFastMove))
                    {
                        commands.Add(_templateService.GeneratePlaneFastMove(point.X, point.Y, settings.Template.PlaneFastMove) + " ; Rapid to start");
                    }
                    else
                    {
                        commands.Add("G0 X" + point.X.ToString("F3") + " Y" + point.Y.ToString("F3") + " ; Rapid move to start");
                    }
                    commands.Add("G0 Z-5 ; Move to depth");
                }
                else
                {
                    if (settings.Template != null && !string.IsNullOrEmpty(settings.Template.PlaneLinearMove))
                    {
                        commands.Add(_templateService.GeneratePlaneLinearMove(point.X, point.Y, settings.FeedRate, settings.Template.PlaneLinearMove));
                    }
                    else
                    {
                        commands.Add("G1 X" + point.X.ToString("F3") + " Y" + point.Y.ToString("F3") + " Z" + point.Z.ToString("F3") + " ; Linear move");
                    }
                }
            }

            return commands;
        }

        private List<string> GenerateLineCommandsWithDepth(Entity entity, bool isFirstPoint, double depth, GcodeModel settings)
        {
            var commands = new List<string>();

            for (int i = 0; i < entity.Points.Count; i++)
            {
                var point = entity.Points[i];

                if (isFirstPoint && i == 0)
                {
                    commands.Add("G0 X" + point.X.ToString("F3") + " Y" + point.Y.ToString("F3") + " ; Rapid to start");
                    commands.Add("G1 Z" + depth.ToString("F3") + " F" + (settings.FeedRate / 2).ToString("F3") + " ; Plunge to depth");
                }
                else
                {
                    commands.Add("G1 X" + point.X.ToString("F3") + " Y" + point.Y.ToString("F3") + " Z" + depth.ToString("F3") + " F" + settings.FeedRate.ToString("F3"));
                }
            }

            return commands;
        }

        private List<string> GenerateCircleCommands(Entity entity, double toolDiameter, GcodeModel settings)
        {
            var commands = new List<string>();

            if (entity.Points.Count > 0)
            {
                var center = entity.Points[0];
                double radius = entity.Radius > 0 ? entity.Radius : toolDiameter / 2;
                
                // Move to start point on circle
                double startX = center.X + radius;
                double startY = center.Y;
                
                commands.Add("G0 X" + startX.ToString("F3") + " Y" + startY.ToString("F3") + " ; Move to circle start");
                commands.Add("G0 Z-5 ; Move to depth");
                
                // Generate full circle as two 180-degree arcs
                if (settings.Template != null && !string.IsNullOrEmpty(settings.Template.CWArcMove))
                {
                    commands.Add(_templateService.GenerateArcMove(
                        startX, startY, -radius, 0, settings.FeedRate, true,
                        settings.Template.CWArcMove, settings.Template.CCWArcMove) + " ; Circle arc 1");
                    commands.Add(_templateService.GenerateArcMove(
                        startX, startY, -radius, 0, settings.FeedRate, true,
                        settings.Template.CWArcMove, settings.Template.CCWArcMove) + " ; Circle arc 2");
                }
                else
                {
                    commands.Add("G2 X" + startX.ToString("F3") + " Y" + startY.ToString("F3") + 
                        " I" + (-radius).ToString("F3") + " J0 ; Circle");
                }
            }

            return commands;
        }

        private List<string> GenerateCircleCommandsWithDepth(Entity entity, double depth, GcodeModel settings)
        {
            var commands = new List<string>();

            if (entity.Points.Count > 0)
            {
                var center = entity.Points[0];
                double radius = entity.Radius > 0 ? entity.Radius : settings.ToolDiameter / 2;
                
                double startX = center.X + radius;
                double startY = center.Y;
                
                commands.Add("G0 X" + startX.ToString("F3") + " Y" + startY.ToString("F3"));
                commands.Add("G1 Z" + depth.ToString("F3") + " F" + (settings.FeedRate / 2).ToString("F3"));
                commands.Add("G2 X" + startX.ToString("F3") + " Y" + startY.ToString("F3") + 
                    " I" + (-radius).ToString("F3") + " J0 Z" + depth.ToString("F3") + " F" + settings.FeedRate.ToString("F3"));
            }

            return commands;
        }

        private List<string> GenerateArcCommands(Entity entity, GcodeModel settings)
        {
            var commands = new List<string>();

            if (entity.Points.Count >= 3)
            {
                var center = entity.Points[0];
                var start = entity.Points[1];
                var end = entity.Points[2];
                
                double i = center.X - start.X;
                double j = center.Y - start.Y;
                
                commands.Add("G0 X" + start.X.ToString("F3") + " Y" + start.Y.ToString("F3"));
                commands.Add("G0 Z-5");
                
                bool clockwise = entity.StartAngle < entity.EndAngle;
                if (settings.Template != null)
                {
                    commands.Add(_templateService.GenerateArcMove(end.X, end.Y, i, j, settings.FeedRate,
                        clockwise, settings.Template.CWArcMove, settings.Template.CCWArcMove));
                }
                else
                {
                    string arcCmd = clockwise ? "G2" : "G3";
                    commands.Add(arcCmd + " X" + end.X.ToString("F3") + " Y" + end.Y.ToString("F3") + 
                        " I" + i.ToString("F3") + " J" + j.ToString("F3") + " F" + settings.FeedRate.ToString("F3"));
                }
            }

            return commands;
        }

        private List<string> GenerateArcCommandsWithDepth(Entity entity, double depth, GcodeModel settings)
        {
            var commands = new List<string>();

            if (entity.Points.Count >= 3)
            {
                var center = entity.Points[0];
                var start = entity.Points[1];
                var end = entity.Points[2];
                
                double i = center.X - start.X;
                double j = center.Y - start.Y;
                
                commands.Add("G0 X" + start.X.ToString("F3") + " Y" + start.Y.ToString("F3"));
                commands.Add("G1 Z" + depth.ToString("F3") + " F" + (settings.FeedRate / 2).ToString("F3"));
                
                bool clockwise = entity.StartAngle < entity.EndAngle;
                string arcCmd = clockwise ? "G2" : "G3";
                commands.Add(arcCmd + " X" + end.X.ToString("F3") + " Y" + end.Y.ToString("F3") + 
                    " I" + i.ToString("F3") + " J" + j.ToString("F3") + " Z" + depth.ToString("F3") + " F" + settings.FeedRate.ToString("F3"));
            }

            return commands;
        }

        private List<string> GeneratePolylineCommands(Entity entity, bool isFirstPoint, GcodeModel settings)
        {
            var commands = new List<string>();

            for (int i = 0; i < entity.Points.Count; i++)
            {
                var point = entity.Points[i];

                if (isFirstPoint && i == 0)
                {
                    commands.Add("G0 X" + point.X.ToString("F3") + " Y" + point.Y.ToString("F3") + " ; Rapid move to start");
                    commands.Add("G0 Z-5 ; Move to depth");
                }
                else
                {
                    commands.Add("G1 X" + point.X.ToString("F3") + " Y" + point.Y.ToString("F3") + " ; Linear move");
                }
            }

            return commands;
        }

        private List<string> GeneratePolylineCommandsWithDepth(Entity entity, bool isFirstPoint, double depth, GcodeModel settings)
        {
            var commands = new List<string>();

            for (int i = 0; i < entity.Points.Count; i++)
            {
                var point = entity.Points[i];

                if (isFirstPoint && i == 0)
                {
                    commands.Add("G0 X" + point.X.ToString("F3") + " Y" + point.Y.ToString("F3"));
                    commands.Add("G1 Z" + depth.ToString("F3") + " F" + (settings.FeedRate / 2).ToString("F3"));
                }
                else
                {
                    commands.Add("G1 X" + point.X.ToString("F3") + " Y" + point.Y.ToString("F3") + " Z" + depth.ToString("F3") + " F" + settings.FeedRate.ToString("F3"));
                }
            }

            return commands;
        }

        public void SaveGCode(string filePath)
        {
            if (_currentGcode == null)
                throw new InvalidOperationException("No G-Code generated");

            try
            {
                File.WriteAllText(filePath, _currentGcode.Code);
            }
            catch (Exception ex)
            {
                throw new Exception("Error saving G-Code: " + ex.Message, ex);
            }
        }

        public GcodeModel GetCurrentGCode()
        {
            return _currentGcode;
        }
    }
}
