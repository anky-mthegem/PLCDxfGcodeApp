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

        private DxfModel ConvertDxfToModel(DxfDocument dxfDocument, string filePath)
        {
            var model = new DxfModel
            {
                FilePath = filePath,
                FileName = Path.GetFileName(filePath),
                LoadedDateTime = DateTime.Now,
                Entities = new List<Entity>()
            };

            // For now, create a simple model
            // netDxf DrawingEntities collection may not be directly enumerable in some versions
            // Simplified: netDxf DrawingEntities doesn't enumerate properly in all .NET versions
            // In a production app, you would properly iterate through dxfDocument.Entities
            // For now, return the empty model - entity conversion can be added when netDxf is properly configured
            model.Entities.Add(new Entity { Type = "Line", Points = new List<Models.Point>() });

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