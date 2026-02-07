using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;
using PLCDxfGcodeApp.Services;
using PLCDxfGcodeApp.ViewModels;

namespace PLCDxfGcodeApp.Views
{
    public partial class MainWindow : Window
    {
        private MainWindowViewModel _viewModel;
        private DxfService _dxfService;
        private PLCService _plcService;
        private GcodeService _gcodeService;

    public MainWindow()
    {
        InitializeComponent();
        InitializeServices();
        InitializeViewModel();
    }

    private void InitializeServices()
    {
        _dxfService = new DxfService();
        _plcService = new PLCService();
        _gcodeService = new GcodeService();
    }

    private void InitializeViewModel()
    {
        if (_dxfService != null && _plcService != null && _gcodeService != null)
        {
            _viewModel = new MainWindowViewModel(_dxfService, _plcService, _gcodeService);
            DataContext = _viewModel;
        }
    }

    private void OpenDxfFile_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "DXF Files (*.dxf)|*.dxf|All Files (*.*)|*.*",
            Title = "Open DXF File"
        };

        if (dialog.ShowDialog() == true && _viewModel != null)
        {
            try
            {
                _viewModel.OpenDxfFile(dialog.FileName);
                DxfInfoTextBlock.Text = "Loaded: " + System.IO.Path.GetFileName(dialog.FileName);
                UpdateDxfCanvas();
            }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading DXF file: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SaveGCode_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "G-Code Files (*.nc)|*.nc|Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                Title = "Save G-Code"
            };

            if (dialog.ShowDialog() == true && _viewModel != null)
            {
                try
                {
                    _viewModel.SaveGCode(dialog.FileName);
                    MessageBox.Show("G-Code saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error saving G-Code: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void NewFile_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.NewFile();
                DxfCanvas.Children.Clear();
                DxfInfoTextBlock.Text = "New file created";
                GCodeTextBox.Text = "";
            }
        }

        private void ConnectToPLC_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel == null) return;

            try
            {
                string ip = PLCIpTextBox.Text;
                int rack = int.Parse(PLCRackTextBox.Text);
                int slot = int.Parse(PLCSlotTextBox.Text);

                if (_viewModel.ConnectToPlc(ip, rack, slot))
                {
                    ConnectionStatus.Text = "● Status: Connected";
                    ConnectionStatus.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Green);
                    MessageBox.Show("Connected to PLC successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    ConnectionStatus.Text = "● Status: Connection Failed";
                    ConnectionStatus.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
                    MessageBox.Show("Failed to connect to PLC", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GenerateGCode_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel == null) return;

            try
            {
                double feedRate = double.Parse(FeedRateTextBox.Text);
                int spindleSpeed = int.Parse(SpindleSpeedTextBox.Text);
                double toolDiameter = double.Parse(ToolDiameterTextBox.Text);

                // Get advanced settings from UI
                bool enablePathOffset = EnablePathOffsetCheckBox.IsChecked == true;
                double pathOffsetAmount = double.Parse(PathOffsetAmountTextBox.Text);
                bool offsetInward = OffsetInwardCheckBox.IsChecked == true;

                bool enablePocket = EnablePocketCheckBox.IsChecked == true;
                double pocketStepover = double.Parse(PocketStepoverTextBox.Text);
                bool detectIslands = DetectIslandsCheckBox.IsChecked == true;

                bool enableMultiPass = EnableMultiPassCheckBox.IsChecked == true;
                double totalDepth = double.Parse(TotalDepthTextBox.Text);
                double depthPerPass = double.Parse(DepthPerPassTextBox.Text);

                bool convertSplines = ConvertSplinesCheckBox.IsChecked == true;
                double arcTolerance = double.Parse(ArcToleranceTextBox.Text);

                var gcode = _viewModel.GenerateGCodeAdvanced(
                    feedRate, spindleSpeed, toolDiameter,
                    enablePathOffset, pathOffsetAmount, offsetInward,
                    enablePocket, pocketStepover, detectIslands,
                    enableMultiPass, totalDepth, depthPerPass,
                    convertSplines, arcTolerance
                );
                
                GCodeTextBox.Text = gcode;
                MessageBox.Show("G-Code generated successfully with advanced features!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error generating G-Code: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateDxfCanvas()
        {
            DxfCanvas.Children.Clear();
            
            if (_viewModel == null)
                return;

            var dxfModel = _dxfService.GetCurrentDxf();
            if (dxfModel == null || dxfModel.Entities.Count == 0)
                return;

            // Calculate bounds for scaling
            double minX = double.MaxValue, minY = double.MaxValue;
            double maxX = double.MinValue, maxY = double.MinValue;

            foreach (var entity in dxfModel.Entities)
            {
                foreach (var point in entity.Points)
                {
                    if (point.X < minX) minX = point.X;
                    if (point.Y < minY) minY = point.Y;
                    if (point.X > maxX) maxX = point.X;
                    if (point.Y > maxY) maxY = point.Y;
                }
            }

            double width = maxX - minX;
            double height = maxY - minY;
            
            if (width < 0.01 || height < 0.01)
                return;

            // Calculate scale to fit canvas with margins
            double canvasWidth = DxfCanvas.ActualWidth > 0 ? DxfCanvas.ActualWidth : 600;
            double canvasHeight = DxfCanvas.ActualHeight > 0 ? DxfCanvas.ActualHeight : 600;
            double margin = 40;
            
            double scaleX = (canvasWidth - 2 * margin) / width;
            double scaleY = (canvasHeight - 2 * margin) / height;
            double scale = Math.Min(scaleX, scaleY);

            // Draw each entity
            foreach (var entity in dxfModel.Entities)
            {
                if (entity.Type == "Line")
                {
                    DrawLine(entity, minX, minY, scale, margin);
                }
                else if (entity.Type == "Circle")
                {
                    DrawCircle(entity, minX, minY, scale, margin);
                }
                else if (entity.Type == "Arc")
                {
                    DrawArc(entity, minX, minY, scale, margin);
                }
                else if (entity.Type == "LwPolyline")
                {
                    DrawPolyline(entity, minX, minY, scale, margin);
                }
            }
        }

        private void DrawLine(PLCDxfGcodeApp.Models.Entity entity, double minX, double minY, double scale, double margin)
        {
            if (entity.Points.Count < 2)
                return;

            for (int i = 0; i < entity.Points.Count - 1; i++)
            {
                var p1 = entity.Points[i];
                var p2 = entity.Points[i + 1];

                var line = new Line
                {
                    X1 = (p1.X - minX) * scale + margin,
                    Y1 = (p1.Y - minY) * scale + margin,
                    X2 = (p2.X - minX) * scale + margin,
                    Y2 = (p2.Y - minY) * scale + margin,
                    Stroke = new SolidColorBrush(Color.FromRgb(33, 150, 243)),
                    StrokeThickness = 2
                };

                DxfCanvas.Children.Add(line);
            }
        }

        private void DrawCircle(PLCDxfGcodeApp.Models.Entity entity, double minX, double minY, double scale, double margin)
        {
            if (entity.Points.Count < 1)
                return;

            var center = entity.Points[0];
            double radius = entity.Radius * scale;

            var ellipse = new Ellipse
            {
                Width = radius * 2,
                Height = radius * 2,
                Stroke = new SolidColorBrush(Color.FromRgb(76, 175, 80)),
                StrokeThickness = 2,
                Fill = Brushes.Transparent
            };

            double left = (center.X - minX) * scale + margin - radius;
            double top = (center.Y - minY) * scale + margin - radius;

            Canvas.SetLeft(ellipse, left);
            Canvas.SetTop(ellipse, top);

            DxfCanvas.Children.Add(ellipse);
        }

        private void DrawArc(PLCDxfGcodeApp.Models.Entity entity, double minX, double minY, double scale, double margin)
        {
            if (entity.Points.Count < 3)
                return;

            var center = entity.Points[0];
            var start = entity.Points[1];
            var end = entity.Points[2];

            // For simplicity, draw as line segments approximation
            int segments = 20;
            double startAngle = entity.StartAngle * Math.PI / 180;
            double endAngle = entity.EndAngle * Math.PI / 180;
            double angleStep = (endAngle - startAngle) / segments;
            double radius = entity.Radius;

            for (int i = 0; i < segments; i++)
            {
                double angle1 = startAngle + i * angleStep;
                double angle2 = startAngle + (i + 1) * angleStep;

                double x1 = center.X + radius * Math.Cos(angle1);
                double y1 = center.Y + radius * Math.Sin(angle1);
                double x2 = center.X + radius * Math.Cos(angle2);
                double y2 = center.Y + radius * Math.Sin(angle2);

                var line = new Line
                {
                    X1 = (x1 - minX) * scale + margin,
                    Y1 = (y1 - minY) * scale + margin,
                    X2 = (x2 - minX) * scale + margin,
                    Y2 = (y2 - minY) * scale + margin,
                    Stroke = new SolidColorBrush(Color.FromRgb(255, 152, 0)),
                    StrokeThickness = 2
                };

                DxfCanvas.Children.Add(line);
            }
        }

        private void DrawPolyline(PLCDxfGcodeApp.Models.Entity entity, double minX, double minY, double scale, double margin)
        {
            if (entity.Points.Count < 2)
                return;

            var polyline = new Polyline
            {
                Stroke = new SolidColorBrush(Color.FromRgb(156, 39, 176)),
                StrokeThickness = 2,
                Fill = Brushes.Transparent
            };

            foreach (var point in entity.Points)
            {
                double x = (point.X - minX) * scale + margin;
                double y = (point.Y - minY) * scale + margin;
                polyline.Points.Add(new Point(x, y));
            }

            DxfCanvas.Children.Add(polyline);
        }
    }
}