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