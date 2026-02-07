# Getting Started Guide

## Project Setup

This is a C# WPF application for Siemens S7-1500 PLC communication with DXF editing and G-code generation.

### Prerequisites

- Windows 10/11
- .NET 8.0 SDK or higher
- Visual Studio Code with C# extension or Visual Studio 2022

### Installation Steps

1. **Open the project** in VS Code or Visual Studio
2. **Restore packages**:
   ```bash
   dotnet restore
   ```
3. **Build the project**:
   ```bash
   dotnet build
   ```
4. **Run the application**:
   ```bash
   dotnet run
   ```

## Architecture Overview

### Project Structure

- **Models/**: Data models for DXF, G-code, and PLC
- **Services/**: Business logic for DXF handling, G-code generation, PLC communication
- **ViewModels/**: MVVM pattern implementation
- **Views/**: WPF XAML UI components

### Key Components

1. **DxfService** - Handles DXF file I/O using netDxf library
2. **GcodeService** - Converts DXF entities to G-code commands
3. **PLCService** - Manages S7-1500 PLC communication via TCP/IP
4. **MainWindowViewModel** - Coordinates between services and UI

## Development Workflow

### Building

```bash
dotnet build
```

### Running

```bash
dotnet run
```

### Debugging

1. In VS Code: Press F5 or use the Debug panel
2. Set breakpoints in your code
3. Use the Debug Console to inspect variables

## Feature Implementation

### Current Features
- DXF file loading (basic support)
- G-code generation from CAD geometries
- PLC connection management
- Real-time UI status updates

### In Development
- DXF canvas visualization
- Advanced G-code optimization
- Multi-PLC support
- Tool path simulation

## Troubleshooting

### Build Issues

**Missing netDxf types:**
- Ensure netDxf v3.0.1 is installed: `dotnet add package netDxf`

**WPF not running on Linux/Mac:**
- This application requires Windows (.NET 8.0-windows target)

### Runtime Issues

**PLC Connection Failed:**
- Check IP address and network connectivity
- Verify firewall allows TCP port 102
- Confirm Rack/Slot numbers match your PLC configuration

**DXF File Not Loading:**
- Ensure DXF file is valid and not corrupted
- Try opening in CAD software first
- Check file permissions

## Next Steps

1. Implement DXF canvas rendering in MainWindow
2. Add advanced G-code post-processors
3. Create unit tests for services
4. Add configuration profiles for different CNC machines
5. Implement tool library management

## Resources

- [netDxf Documentation](https://github.com/haplokuon/netDxf)
- [Siemens S7 Communication](https://support.industry.siemens.com/)
- [G-Code Reference](https://en.wikipedia.org/wiki/G-code)
- [WPF Documentation](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/)

---

**Need Help?** Check the README.md for more details or contact the development team.
