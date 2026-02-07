# PLC DXF G-Code Application

A Windows desktop application for Siemens S7-1500 PLC communication with DXF file editing and G-code generation capabilities.

## Features

- **DXF File Editing**: Open, view, and edit DXF (Drawing Exchange Format) files
- **PLC Communication**: Connect to Siemens S7-1500 PLCs using Snap7
- **G-Code Generation**: Automatically convert DXF geometries to CNC G-code
- **Real-time Monitoring**: Status indicators and connection management
- **Professional UI**: WPF-based user interface with intuitive controls

## System Requirements

- Windows 10/11
- .NET 8.0 or higher
- Siemens S7-1500 PLC (or compatible)

## Project Structure

```
PLCDxfGcodeApp/
├── Models/              # Data models (DxfModel, GcodeModel, PLCModel)
├── Services/            # Business logic (DxfService, GcodeService, PLCService)
├── ViewModels/          # MVVM ViewModels
├── Views/               # WPF XAML views
├── Utils/               # Utility classes and helpers
├── App.xaml(.cs)        # Application entry point
└── PLCDxfGcodeApp.csproj # Project configuration
```

## Installation & Setup

### Prerequisites

Install the required packages via NuGet:
- netDxf (DXF file handling)
- Snap7 (PLC communication)
- MvvmLight (MVVM framework)

### Build Instructions

1. Open the project in Visual Studio 2022 or VS Code with C# extension
2. Restore NuGet packages: `dotnet restore`
3. Build the project: `dotnet build`
4. Run the application: `dotnet run`

## Usage

### Opening DXF Files

1. Click **Open DXF File**
2. Select a DXF file from your computer
3. The file will be displayed in the DXF Editor canvas

### PLC Connection

1. Enter PLC IP Address (e.g., 192.168.1.100)
2. Set Rack and Slot numbers (default: 0 and 1)
3. Click **Connect to PLC**
4. Status will show "Connected" when successful

### Generating G-Code

1. Load a DXF file
2. Set cutting parameters:
   - Feed Rate (mm/min)
   - Spindle Speed (RPM)
   - Tool Diameter (mm)
3. Click **Generate G-Code**
4. Preview G-Code in the output panel
5. Click **Save G-Code** to save to file

### Sending to PLC

After generating G-Code:
1. Ensure PLC is connected
2. G-Code can be sent to PLC via the PLCService
3. Monitor status messages for confirmation

## Technology Stack

- **UI Framework**: WPF (Windows Presentation Foundation)
- **Language**: C# 12
- **DXF Library**: netDxf 2.16.0
- **PLC Communication**: Snap7 1.4.2
- **MVVM Framework**: MvvmLight 5.4.1.1
- **.NET Version**: 8.0

## Configuration

### PLC Parameters
- **IP Address**: Target S7-1500 PLC IP
- **Rack**: CPU rack number (typically 0)
- **Slot**: CPU slot number (typically 1)

### G-Code Settings
- **Feed Rate**: Cutting speed (mm/minute)
- **Spindle Speed**: Rotation speed (RPM)
- **Tool Diameter**: Tool size for offset calculations

## API Reference

### DxfService
```csharp
// Load DXF file
DxfModel? model = dxfService.LoadDxfFile(filePath);

// Create new file
DxfModel newModel = dxfService.NewFile();

// Save DXF file
dxfService.SaveDxfFile(filePath);
```

### PLCService
```csharp
// Connect to PLC
bool connected = plcService.Connect(ipAddress, rack, slot);

// Write data to PLC
bool success = plcService.WriteData(dbNumber, offset, data);

// Read data from PLC
byte[]? data = plcService.ReadData(dbNumber, offset, count);

// Send G-Code to PLC
bool success = plcService.WriteGCode(gcode);
```

### GcodeService
```csharp
// Generate G-Code from DXF
string gcode = gcodeService.GenerateGCode(dxfModel, feedRate, spindleSpeed, toolDiameter);

// Save G-Code to file
gcodeService.SaveGCode(filePath);
```

## Troubleshooting

### Cannot Connect to PLC
- Verify IP address and network connectivity
- Check Rack and Slot settings
- Ensure Snap7 is properly installed
- Firewall may be blocking connection (port 102)

### DXF File Not Loading
- Ensure file is a valid DXF format
- Check file permissions
- Try saving DXF in ASCII format from CAD program

### G-Code Not Generating
- Load a valid DXF file first
- Ensure DXF contains supported entities (lines, circles, polylines)
- Check cutting parameters are valid

## Future Enhancements

- [ ] Visual DXF canvas rendering
- [ ] Real-time PLC data monitoring dashboard
- [ ] Advanced G-code post-processors
- [ ] Tool path simulation
- [ ] Multi-PLC support
- [ ] Configuration profiles
- [ ] G-code optimization algorithms
- [ ] Export to multiple NC file formats

## License

This project is provided as-is for personal and commercial use.

## Support & Contributing

For issues, feature requests, or contributions, please contact the development team.

---

**Version**: 1.0.0  
**Last Updated**: February 2026
