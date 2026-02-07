using System;
using System.Collections.Generic;
using System.IO;
using netDxf;
using netDxf.Entities;
using PLCDxfGcodeApp.Models;

namespace PLCDxfGcodeApp.Services
{
    public class DxfService
    {
        private DxfModel _currentModel;

        public DxfModel LoadDxfFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("DXF file not found: " + filePath);

            try
            {
                var dxfDocument = DxfDocument.Load(filePath);
                _currentModel = ConvertDxfToModel(dxfDocument, filePath);
                return _currentModel;
            }
            catch (Exception ex)
            {
                throw new Exception("Error loading DXF file: " + ex.Message, ex);
            }
        }

        public void SaveDxfFile(string filePath)
        {
            if (_currentModel == null)
                throw new InvalidOperationException("No DXF model loaded");

            try
            {
                var dxfDocument = ConvertModelToDxf(_currentModel);
                dxfDocument.Save(filePath);
            }
            catch (Exception ex)
            {
                throw new Exception("Error saving DXF file: " + ex.Message, ex);
            }
        }

        public DxfModel NewFile()
        {
            _currentModel = new DxfModel
            {
                FilePath = null,
                FileName = "Untitled.dxf",
                LoadedDateTime = DateTime.Now,
                Entities = new List<Entity>()
            };
            return _currentModel;
        }

        public DxfModel GetCurrentModel()
        {
            return _currentModel;
        }
        
        public DxfModel GetCurrentDxf()
        {
            return _currentModel;
        }

        private DxfModel ConvertDxfToModel(DxfDocument dxfDocument, string filePath)
        {
            var model = new DxfModel
            {
                FilePath = filePath,
                FileName = Path.GetFileName(filePath),
                LoadedDateTime = DateTime.Now,
                Entities = new List<Entity>()
            };

            try
            {
                // Extract Lines
                if (dxfDocument.Entities != null && dxfDocument.Entities.Lines != null)
                {
                    foreach (var line in dxfDocument.Entities.Lines)
                    {
                        var entity = new Entity { Type = "Line", Points = new List<Models.Point>() };
                        entity.Points.Add(new Models.Point { X = line.StartPoint.X, Y = line.StartPoint.Y, Z = line.StartPoint.Z });
                        entity.Points.Add(new Models.Point { X = line.EndPoint.X, Y = line.EndPoint.Y, Z = line.EndPoint.Z });
                        model.Entities.Add(entity);
                    }
                }

                // Extract Circles
                if (dxfDocument.Entities != null && dxfDocument.Entities.Circles != null)
                {
                    foreach (var circle in dxfDocument.Entities.Circles)
                    {
                        var entity = new Entity 
                        { 
                            Type = "Circle", 
                            Points = new List<Models.Point>(),
                            Radius = circle.Radius
                        };
                        entity.Points.Add(new Models.Point { X = circle.Center.X, Y = circle.Center.Y, Z = circle.Center.Z });
                        model.Entities.Add(entity);
                    }
                }

                // Extract Arcs
                if (dxfDocument.Entities != null && dxfDocument.Entities.Arcs != null)
                {
                    foreach (var arc in dxfDocument.Entities.Arcs)
                    {
                        var entity = new Entity 
                        { 
                            Type = "Arc", 
                            Points = new List<Models.Point>(),
                            Radius = arc.Radius,
                            StartAngle = arc.StartAngle,
                            EndAngle = arc.EndAngle
                        };
                        entity.Points.Add(new Models.Point { X = arc.Center.X, Y = arc.Center.Y, Z = arc.Center.Z });
                        
                        // Add start and end points for visualization
                        double startRad = arc.StartAngle * Math.PI / 180;
                        double endRad = arc.EndAngle * Math.PI / 180;
                        entity.Points.Add(new Models.Point 
                        { 
                            X = arc.Center.X + arc.Radius * Math.Cos(startRad), 
                            Y = arc.Center.Y + arc.Radius * Math.Sin(startRad), 
                            Z = arc.Center.Z 
                        });
                        entity.Points.Add(new Models.Point 
                        { 
                            X = arc.Center.X + arc.Radius * Math.Cos(endRad), 
                            Y = arc.Center.Y + arc.Radius * Math.Sin(endRad), 
                            Z = arc.Center.Z 
                        });
                        model.Entities.Add(entity);
                    }
                }

                // Extract Polylines
                if (dxfDocument.Entities != null && dxfDocument.Entities.Polylines2D != null)
                {
                    foreach (var polyline in dxfDocument.Entities.Polylines2D)
                    {
                        var entity = new Entity { Type = "LwPolyline", Points = new List<Models.Point>() };
                        foreach (var vertex in polyline.Vertexes)
                        {
                            entity.Points.Add(new Models.Point { X = vertex.Position.X, Y = vertex.Position.Y, Z = 0 });
                        }
                        if (entity.Points.Count > 0)
                            model.Entities.Add(entity);
                    }
                }

                Console.WriteLine("Loaded DXF: " + model.Entities.Count + " entities");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error converting DXF entities: " + ex.Message);
            }

            return model;
        }

        private void ConvertLine(Line line, Entity entity)
        {
            entity.Points.Add(new Models.Point { X = line.StartPoint.X, Y = line.StartPoint.Y, Z = line.StartPoint.Z });
            entity.Points.Add(new Models.Point { X = line.EndPoint.X, Y = line.EndPoint.Y, Z = line.EndPoint.Z });
        }

        private void ConvertCircle(Circle circle, Entity entity)
        {
            entity.Points.Add(new Models.Point { X = circle.Center.X, Y = circle.Center.Y, Z = circle.Center.Z });
        }

        private void ConvertArc(Arc arc, Entity entity)
        {
            entity.Points.Add(new Models.Point { X = arc.Center.X, Y = arc.Center.Y, Z = arc.Center.Z });
        }

        private DxfDocument ConvertModelToDxf(DxfModel model)
        {
            var dxfDocument = new DxfDocument();

            foreach (var entity in model.Entities)
            {
                if (entity.Type == "Line" && entity.Points.Count >= 2)
                {
                    var line = new Line(
                        new netDxf.Vector3(entity.Points[0].X, entity.Points[0].Y, entity.Points[0].Z),
                        new netDxf.Vector3(entity.Points[1].X, entity.Points[1].Y, entity.Points[1].Z));
                    dxfDocument.Entities.Add(line);
                }
            }

            return dxfDocument;
        }
    }
}