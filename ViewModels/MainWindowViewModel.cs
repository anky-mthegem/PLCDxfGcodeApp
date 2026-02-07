using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using PLCDxfGcodeApp.Models;
using PLCDxfGcodeApp.Services;

namespace PLCDxfGcodeApp.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly DxfService _dxfService;
        private readonly PLCService _plcService;
        private readonly GcodeService _gcodeService;

        private DxfModel _currentDxfModel;
        private GcodeModel _currentGcodeModel;
        private string _statusMessage = "Ready";
        private bool _isConnected;

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindowViewModel(DxfService dxfService, PLCService plcService, GcodeService gcodeService)
        {
            _dxfService = dxfService;
            _plcService = plcService;
            _gcodeService = gcodeService;
        }

        public string StatusMessage
        {
        get => _statusMessage;
        set { _statusMessage = value; OnPropertyChanged(); }
    }

    public bool IsConnected
    {
        get => _isConnected;
        set { _isConnected = value; OnPropertyChanged(); }
    }

    public void OpenDxfFile(string filePath)
    {
        try
        {
            _currentDxfModel = _dxfService.LoadDxfFile(filePath);
            int count = _currentDxfModel != null ? _currentDxfModel.Entities.Count : 0;
            StatusMessage = "Loaded: " + System.IO.Path.GetFileName(filePath) + " - Entities: " + count;
        }
        catch (Exception ex)
        {
            StatusMessage = "Error: " + ex.Message;
            throw;
        }
    }

    public void SaveGCode(string filePath)
    {
        try
        {
            _gcodeService.SaveGCode(filePath);
            StatusMessage = "G-Code saved: " + System.IO.Path.GetFileName(filePath);
        }
        catch (Exception ex)
        {
            StatusMessage = "Error: " + ex.Message;
            throw;
        }
    }

    public void NewFile()
    {
        _currentDxfModel = _dxfService.NewFile();
        StatusMessage = "New file created";
    }

    public bool ConnectToPlc(string ipAddress, int rack, int slot)
    {
        try
        {
            bool connected = _plcService.Connect(ipAddress, rack, slot);
            IsConnected = connected;
            StatusMessage = connected ? "Connected to PLC" : "Failed to connect to PLC";
            return connected;
        }
        catch (Exception ex)
        {
            StatusMessage = "Connection Error: " + ex.Message;
            return false;
        }
    }

    public string GenerateGCode(double feedRate, int spindleSpeed, double toolDiameter)
    {
        try
        {
            if (_currentDxfModel == null)
                throw new InvalidOperationException("No DXF file loaded");

            string gcode = _gcodeService.GenerateGCode(_currentDxfModel, feedRate, spindleSpeed, toolDiameter);
            StatusMessage = "G-Code generated successfully";
            return gcode;
        }
        catch (Exception ex)
        {
            StatusMessage = "Error: " + ex.Message;
            throw;
        }
    }

    public void SendGCodeToPLC(string gcode)
    {
        try
        {
            if (!IsConnected)
                throw new InvalidOperationException("PLC not connected");

            bool success = _plcService.WriteGCode(gcode);
            StatusMessage = success ? "G-Code sent to PLC" : "Failed to send G-Code to PLC";
        }
        catch (Exception ex)
        {
            StatusMessage = "Error: " + ex.Message;
        }
    }

protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
