using System;
using System.Collections.Generic;
using System.IO;
using PLCDxfGcodeApp.Models;

namespace PLCDxfGcodeApp.Services
{
    public class GcodeService
    {
        private GcodeModel _currentGcode;

        public string GenerateGCode(DxfModel dxfModel, double feedRate, int spindleSpeed, double toolDiameter)
        {
            if (dxfModel == null || dxfModel.Entities.Count == 0)
                throw new InvalidOperationException("No DXF entities to convert");

            var commands = new List<string>();

            // G-Code header
            commands.Add("; Generated G-Code from DXF");
            commands.Add("; Feed Rate: " + feedRate + " mm/min");
            commands.Add("; Spindle Speed: " + spindleSpeed + " RPM");
            commands.Add("; Tool Diameter: " + toolDiameter + " mm");
            commands.Add("; ");
            commands.Add("G21 ; Metric");
            commands.Add("G90 ; Absolute positioning");
            commands.Add("S" + spindleSpeed + " ; Set spindle speed");
            commands.Add("M3 ; Spindle on");
            commands.Add("F" + feedRate + " ; Set feed rate");
            commands.Add("G0 Z10 ; Move to safe height");

            // Convert entities to G-Code
            bool isFirstPoint = true;
            foreach (var entity in dxfModel.Entities)
            {
                if (entity.Points.Count < 1) continue;

                if (entity.Type == "Line")
                {
                    commands.AddRange(GenerateLineCommands(entity, isFirstPoint));
                    isFirstPoint = false;
                }
                else if (entity.Type == "Circle")
                {
                    commands.AddRange(GenerateCircleCommands(entity, toolDiameter));
                }
                else if (entity.Type == "LwPolyline")
                {
                    commands.AddRange(GeneratePolylineCommands(entity, isFirstPoint));
                    isFirstPoint = false;
                }
            }

            // G-Code footer
            commands.Add("G0 Z10 ; Move to safe height");
            commands.Add("M5 ; Spindle off");
            commands.Add("M30 ; Program end");

            var gcode = string.Join("\n", commands);
            _currentGcode = new GcodeModel
            {
                Code = gcode,
                FeedRate = feedRate,
                SpindleSpeed = spindleSpeed,
                ToolDiameter = toolDiameter,
                GeneratedDateTime = DateTime.Now
            };

            return gcode;
        }

        private List<string> GenerateLineCommands(Entity entity, bool isFirstPoint)
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
                    commands.Add("G1 X" + point.X.ToString("F3") + " Y" + point.Y.ToString("F3") + " Z" + point.Z.ToString("F3") + " ; Linear move");
                }
            }

            return commands;
        }

        private List<string> GenerateCircleCommands(Entity entity, double toolDiameter)
        {
            var commands = new List<string>();

            if (entity.Points.Count > 0)
            {
                var center = entity.Points[0];
                commands.Add("G0 X" + center.X.ToString("F3") + " Y" + center.Y.ToString("F3") + " ; Move to circle center");
                commands.Add("G0 Z-5 ; Move to depth");
            }

            return commands;
        }

        private List<string> GeneratePolylineCommands(Entity entity, bool isFirstPoint)
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
