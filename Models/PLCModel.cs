using System;

namespace PLCDxfGcodeApp.Models
{
    public class PLCConnection
{
    public string IpAddress { get; set; } = string.Empty;
    public int Rack { get; set; }
    public int Slot { get; set; }
    public bool IsConnected { get; set; }
    public DateTime ConnectedDateTime { get; set; }
}

public class PLCData
{
    public string Tag { get; set; } = string.Empty;
        public object Value { get; set; }
        public string DataType { get; set; } = string.Empty;
    }
}
