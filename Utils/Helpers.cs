using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PLCDxfGcodeApp.Utils
{
    public static class GcodeHelper
    {
        public static string FormatCoordinate(double value, int decimals = 3)
        {
            return value.ToString("F" + decimals);
        }

        public static string Comment(string text)
        {
            return "; " + text;
        }

        public static string RapidMove(double x, double y, double z = 0)
        {
            return "G0 X" + x.ToString("F3") + " Y" + y.ToString("F3") + " Z" + z.ToString("F3");
        }

        public static string LinearMove(double x, double y, double z = 0)
        {
            return "G1 X" + x.ToString("F3") + " Y" + y.ToString("F3") + " Z" + z.ToString("F3");
        }

        public static string ArcClockwise(double x, double y, double i, double j, double z = 0)
        {
            return "G2 X" + x.ToString("F3") + " Y" + y.ToString("F3") + " I" + i.ToString("F3") + " J" + j.ToString("F3") + " Z" + z.ToString("F3");
        }

        public static string ArcCounterClockwise(double x, double y, double i, double j, double z = 0)
        {
            return "G3 X" + x.ToString("F3") + " Y" + y.ToString("F3") + " I" + i.ToString("F3") + " J" + j.ToString("F3") + " Z" + z.ToString("F3");
        }

        public static double Distance(double x1, double y1, double x2, double y2)
        {
            double dx = x2 - x1;
            double dy = y2 - y1;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        public static bool ValidateGCode(string gcode)
        {
            if (gcode == null || gcode.Length == 0)
                return false;

            string[] lines = gcode.Split('\n');
            foreach (string line in lines)
            {
                string trimmed = line.Trim();
                if (trimmed.StartsWith(";"))
                    continue;
                if (trimmed.Length == 0)
                    continue;

                char first = char.ToUpper(trimmed[0]);
                if (!"GMSFXYZIJKRABCDHPTEO".Contains(first.ToString()))
                    return false;
            }

            return true;
        }
    }

    public static class UnitConverter
    {
        public static double MillimetersToInches(double mm)
        {
            return mm / 25.4;
        }

        public static double InchesToMillimeters(double inches)
        {
            return inches * 25.4;
        }

        public static double RpmToRadPerSecond(double rpm)
        {
            return rpm * Math.PI / 30;
        }

        public static double RadPerSecondToRpm(double radPerSec)
        {
            return radPerSec * 30 / Math.PI;
        }
    }

    public static class Logger
    {
        private static List<string> _logs = new List<string>();
        private static readonly object _lockObject = new object();

        public enum LogLevel
        {
            Info,
            Warning,
            Error,
            Debug
        }

        public static void Log(string message, LogLevel level = LogLevel.Info)
        {
            lock (_lockObject)
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                string logEntry = "[" + timestamp + "] [" + level + "] " + message;
                _logs.Add(logEntry);
                Console.WriteLine(logEntry);
            }
        }

        public static List<string> GetLogs()
        {
            lock (_lockObject)
            {
                return new List<string>(_logs);
            }
        }

        public static void ClearLogs()
        {
            lock (_lockObject)
            {
                _logs.Clear();
            }
        }

        public static void ExportLogs(string filePath)
        {
            lock (_lockObject)
            {
                File.WriteAllLines(filePath, _logs);
            }
        }
    }
}
