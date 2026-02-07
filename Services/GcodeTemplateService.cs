using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PLCDxfGcodeApp.Services
{
    /// <summary>
    /// G-Code Template Service for Customizable Command Formatting
    /// Based on dxfplotter's template system with variable substitution
    /// Supports formatting like: "G0 X{X:F3} Y{Y:F3}" with variables
    /// </summary>
    public class GcodeTemplateService
    {
        public string FormatCommand(string template, Dictionary<string, object> variables)
        {
            if (string.IsNullOrEmpty(template))
                return string.Empty;

            string result = template;

            // Process each variable in the dictionary
            foreach (var kvp in variables)
            {
                var key = kvp.Key;
                var value = kvp.Value;

                // Find patterns like {X:F3} or {X}
                var pattern = "\\{" + key + "(?::([^}]+))?\\}";
                var matches = Regex.Matches(result, pattern);

                foreach (Match match in matches)
                {
                    string format = null;
                    if (match.Groups.Count > 1 && match.Groups[1].Success)
                    {
                        format = match.Groups[1].Value;
                    }

                    string replacement = FormatValue(value, format);
                    result = result.Replace(match.Value, replacement);
                }
            }

            return result;
        }

        private string FormatValue(object value, string format)
        {
            if (value == null)
                return string.Empty;

            if (format == null)
                return value.ToString();

            // Handle numeric formatting
            if (value is double)
            {
                double d = (double)value;
                return d.ToString(format);
            }
            else if (value is int)
            {
                int i = (int)value;
                return i.ToString(format);
            }
            else if (value is float)
            {
                float f = (float)value;
                return f.ToString(format);
            }

            return value.ToString();
        }

        public string GeneratePlaneFastMove(double x, double y, string template)
        {
            var vars = new Dictionary<string, object>
            {
                { "X", x },
                { "Y", y }
            };
            return FormatCommand(template, vars);
        }

        public string GeneratePlaneLinearMove(double x, double y, double feedRate, string template)
        {
            var vars = new Dictionary<string, object>
            {
                { "X", x },
                { "Y", y },
                { "F", feedRate }
            };
            return FormatCommand(template, vars);
        }

        public string GenerateDepthFastMove(double z, string template)
        {
            var vars = new Dictionary<string, object>
            {
                { "Z", z }
            };
            return FormatCommand(template, vars);
        }

        public string GenerateDepthLinearMove(double z, double feedRate, string template)
        {
            var vars = new Dictionary<string, object>
            {
                { "Z", z },
                { "F", feedRate }
            };
            return FormatCommand(template, vars);
        }

        public string GenerateArcMove(double x, double y, double i, double j, double feedRate, 
            bool clockwise, string cwTemplate, string ccwTemplate)
        {
            var template = clockwise ? cwTemplate : ccwTemplate;
            var vars = new Dictionary<string, object>
            {
                { "X", x },
                { "Y", y },
                { "I", i },
                { "J", j },
                { "F", feedRate }
            };
            return FormatCommand(template, vars);
        }

        public string GenerateSpindleControl(int speed, string preCutTemplate, string postCutTemplate, bool start)
        {
            if (start)
            {
                var vars = new Dictionary<string, object>
                {
                    { "S", speed }
                };
                return FormatCommand(preCutTemplate, vars);
            }
            else
            {
                return FormatCommand(postCutTemplate, new Dictionary<string, object>());
            }
        }

        public List<string> ApplyTemplate(List<string> commands, string template)
        {
            var formatted = new List<string>();

            foreach (var cmd in commands)
            {
                // Parse existing G-code command
                var parsed = ParseGcodeCommand(cmd);
                if (parsed != null)
                {
                    var formatted_cmd = FormatCommand(template, parsed);
                    formatted.Add(formatted_cmd);
                }
                else
                {
                    formatted.Add(cmd);
                }
            }

            return formatted;
        }

        private Dictionary<string, object> ParseGcodeCommand(string command)
        {
            var vars = new Dictionary<string, object>();

            // Extract X, Y, Z, F, S values from G-code
            var patterns = new Dictionary<string, string>
            {
                { "X", @"X([-+]?[0-9]*\.?[0-9]+)" },
                { "Y", @"Y([-+]?[0-9]*\.?[0-9]+)" },
                { "Z", @"Z([-+]?[0-9]*\.?[0-9]+)" },
                { "I", @"I([-+]?[0-9]*\.?[0-9]+)" },
                { "J", @"J([-+]?[0-9]*\.?[0-9]+)" },
                { "F", @"F([-+]?[0-9]*\.?[0-9]+)" },
                { "S", @"S([-+]?[0-9]*\.?[0-9]+)" }
            };

            foreach (var pattern in patterns)
            {
                var match = Regex.Match(command, pattern.Value);
                if (match.Success)
                {
                    double value;
                    if (double.TryParse(match.Groups[1].Value, out value))
                    {
                        vars[pattern.Key] = value;
                    }
                }
            }

            return vars.Count > 0 ? vars : null;
        }
    }
}
