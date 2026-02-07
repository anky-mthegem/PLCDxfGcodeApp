using System;

namespace PLCDxfGcodeApp.Models
{
    public class GcodeModel
{
    public string Code { get; set; } = string.Empty;
    public double FeedRate { get; set; }
    public int SpindleSpeed { get; set; }
    public double ToolDiameter { get; set; }
    public DateTime GeneratedDateTime { get; set; }
    
    // Advanced Features from dxfplotter
    public bool EnablePathOffset { get; set; }
    public double PathOffsetAmount { get; set; }
    public bool OffsetInward { get; set; }
    
    public bool EnablePocketGeneration { get; set; }
    public double PocketStepover { get; set; }
    public bool DetectIslands { get; set; }
    
    public bool EnableMultiPass { get; set; }
    public double TotalDepth { get; set; }
    public double DepthPerPass { get; set; }
    
    public bool ConvertSplinesToArcs { get; set; }
    public double ArcTolerance { get; set; }
    
    public GcodeTemplate Template { get; set; }
}

public class GcodeTemplate
{
    public string PreCut { get; set; } = "M4 S{S}";
    public string PostCut { get; set; } = "M5";
    public string PlaneFastMove { get; set; } = "G0 X{X:F3} Y{Y:F3}";
    public string PlaneLinearMove { get; set; } = "G1 X{X:F3} Y{Y:F3} F{F:F3}";
    public string DepthFastMove { get; set; } = "G0 Z{Z:F3}";
    public string DepthLinearMove { get; set; } = "G1 Z{Z:F3} F{F:F3}";
    public string CWArcMove { get; set; } = "G2 X{X:F3} Y{Y:F3} I{I:F3} J{J:F3} F{F:F3}";
    public string CCWArcMove { get; set; } = "G3 X{X:F3} Y{Y:F3} I{I:F3} J{J:F3} F{F:F3}";
}

public class GcodeCommand
{
    public string Command { get; set; } = string.Empty;
    public double? X { get; set; }
    public double? Y { get; set; }
    public double? Z { get; set; }
    public double? FeedRate { get; set; }
    public int? Speed { get; set; }
}}