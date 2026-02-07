# PLC Communication Guide

## Overview

The PLCService provides communication with Siemens S7-1500 PLCs using ISO-on-TCP (RFC 1006) protocol over TCP/IP.

## Connection Setup

### Parameters

| Parameter | Description | Example |
|-----------|-------------|---------|
| IP Address | S7-1500 IP on network | 192.168.1.100 |
| Rack | CPU rack number | 0 (most common) |
| Slot | CPU slot number | 1 (most common) |
| Port | Fixed TCP port | 102 (standard) |

### Connection Flow

```csharp
var plcService = new PLCService();
bool connected = plcService.Connect("192.168.1.100", 0, 1);

if (connected)
{
    // Send data
    byte[] data = Encoding.UTF8.GetBytes(gcode);
    plcService.WriteData(1, 0, data);
    
    // Disconnect
    plcService.Disconnect();
}
```

## Data Operations

### Writing Data to PLC

```csharp
public bool WriteData(int dbNumber, int offset, byte[] data)
```

**Parameters:**
- `dbNumber`: Data block number (1-255)
- `offset`: Byte offset within the data block
- `data`: Byte array to write

**Example:**
```csharp
byte[] gcode = Encoding.UTF8.GetBytes("G0 X10 Y10 Z0");
plcService.WriteData(1, 0, gcode);  // Write to DB1, offset 0
```

### Reading Data from PLC

```csharp
public byte[]? ReadData(int dbNumber, int offset, int count)
```

**Parameters:**
- `dbNumber`: Data block number to read from
- `offset`: Starting byte offset
- `count`: Number of bytes to read

**Example:**
```csharp
byte[]? data = plcService.ReadData(1, 0, 100);  // Read 100 bytes from DB1
if (data != null)
{
    string text = Encoding.UTF8.GetString(data);
}
```

## G-Code Transmission

### Sending G-Code to PLC

```csharp
public bool WriteGCode(string gcode)
```

The complete G-code program is sent to DB1 of the PLC.

**Example:**
```csharp
string gcodeProgram = @"G21
G90
S1000
M3
F100
G0 Z10
G0 X10 Y10
G0 Z-5
G1 X20 Y10
G0 Z10
M5
M30";

bool success = plcService.WriteGCode(gcodeProgram);
```

## Connection Status

### Checking Connection

```csharp
bool isConnected = plcService.IsConnected();
PLCConnection? connection = plcService.GetConnection();

if (connection != null)
{
    Console.WriteLine($"IP: {connection.IpAddress}");
    Console.WriteLine($"Connected: {connection.ConnectedDateTime}");
}
```

## Error Handling

```csharp
try
{
    plcService.Connect(ipAddress, rack, slot);
}
catch (InvalidOperationException ex)
{
    // PLC already connected or communication error
    Console.WriteLine($"Error: {ex.Message}");
}
catch (Exception ex)
{
    // Network or other exception
    Console.WriteLine($"Unexpected error: {ex.Message}");
}
```

## Protocol Details

### ISO-on-TCP (RFC 1006)

The application uses the ISO-on-TCP protocol for S7-1500 communication:

1. **TCP Connection**: Establishes TCP socket on port 102
2. **COTP Negotiation**: Connection-Oriented Transport Protocol setup
3. **S7 Protocol**: Siemens proprietary protocol over COTP
4. **Data Exchange**: Read/write operations through S7 messages

### Data Block Structure

```
Database (DB1):
┌─────────────────────────────┐
│ Offset 0: Data Start        │
│ Offset n: Data...           │
│ ...                         │
└─────────────────────────────┘
```

## Troubleshooting

### Connection Issues

**Problem**: Cannot connect to PLC
- Check IP address and network connectivity
- Verify PLC is powered and online
- Ensure port 102 is not blocked by firewall
- Confirm Rack/Slot settings match PLC configuration

**Problem**: Connection times out
- Increase timeout value (if supported)
- Check network latency
- Verify PLC is responding to ping

### Data Transfer Issues

**Problem**: Data write fails
- Verify DB number and offset are valid
- Check data size doesn't exceed DB size
- Ensure PLC data block is not protected

**Problem**: Data read returns null
- Confirm data exists in specified DB/offset
- Check read offset and count are valid
- Verify data block is accessible

## Advanced Features (Future)

- [ ] Event-triggered data transfers
- [ ] Real-time monitoring dashboard
- [ ] Multi-PLC failover
- [ ] Data logging and history
- [ ] Diagnostic functions (Dr)
- [ ] Security: Encrypted communication

## Security Considerations

**Production Recommendations:**
1. Use VPN or secure network connection
2. Implement authentication mechanisms
3. Monitor all PLC access attempts
4. Regular security audits
5. Firewall rules: Allow only necessary IP addresses
6. Keep firmware updated

## Testing

### Unit Test Example

```csharp
[Test]
public void TestPLCConnection()
{
    var plcService = new PLCService();
    bool result = plcService.Connect("192.168.1.100", 0, 1);
    Assert.IsTrue(result || !result); // Connection attempt made
}
```

---

For more information, see [README.md](README.md) and [GETTING_STARTED.md](GETTING_STARTED.md)
