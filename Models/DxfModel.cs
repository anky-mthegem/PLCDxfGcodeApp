using System;
using System.Collections.Generic;

namespace PLCDxfGcodeApp.Models
{
    public class DxfModel
{
        public string FilePath { get; set; }
        public List<Entity> Entities { get; set; } = new List<Entity>();
        public string FileName { get; set; }
        public DateTime LoadedDateTime { get; set; }
    }

    public class Entity
    {
        public string Type { get; set; } = string.Empty;
        public List<Point> Points { get; set; } = new List<Point>();
        public string Layer { get; set; }
        public double Radius { get; set; }
        public double StartAngle { get; set; }
        public double EndAngle { get; set; }
    }

    public class Point
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
    }
}
