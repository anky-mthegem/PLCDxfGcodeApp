using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
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
                    DxfInfoTextBlock.Text = "Loaded: " + Path.GetFileName(dialog.FileName);
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
                    ConnectionStatus.Text = "Status: Connected";
                    ConnectionStatus.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Green);
                    MessageBox.Show("Connected to PLC successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    ConnectionStatus.Text = "Status: Connection Failed";
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

                var gcode = _viewModel.GenerateGCode(feedRate, spindleSpeed, toolDiameter);
                GCodeTextBox.Text = gcode;
                MessageBox.Show("G-Code generated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error generating G-Code: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateDxfCanvas()
        {
            DxfCanvas.Children.Clear();
            // Canvas drawing will be implemented in visualization service
        }
    }
}