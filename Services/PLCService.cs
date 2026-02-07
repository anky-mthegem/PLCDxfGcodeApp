using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using PLCDxfGcodeApp.Models;

namespace PLCDxfGcodeApp.Services
{
    /// <summary>
    /// PLCService for Siemens S7-1500 communication via ISO-on-TCP (RFC 1006)
    /// Note: For production use, consider using licensed Siemens S7 communication libraries or Snap7
    /// </summary>
    public class PLCService
    {
        private TcpClient _client;
        private NetworkStream _networkStream;
        private PLCConnection _connection;
        private const int ISO_TCP_PORT = 102;

        public bool Connect(string ipAddress, int rack, int slot)
        {
            try
            {
                _client = new TcpClient();
                _client.Connect(IPAddress.Parse(ipAddress), ISO_TCP_PORT);
                _networkStream = _client.GetStream();

                if (_client.Connected)
                {
                    _connection = new PLCConnection
                    {
                        IpAddress = ipAddress,
                        Rack = rack,
                        Slot = slot,
                        IsConnected = true,
                        ConnectedDateTime = DateTime.Now
                    };
                    Console.WriteLine("Connected to PLC at " + ipAddress);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("PLC Connection Error: " + ex.Message);
                return false;
            }
        }

        public bool Disconnect()
        {
            try
            {
                if (_networkStream != null)
                    _networkStream.Close();
                if (_client != null)
                    _client.Close();
                if (_connection != null)
                    _connection.IsConnected = false;
                Console.WriteLine("Disconnected from PLC");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("PLC Disconnection Error: " + ex.Message);
                return false;
            }
        }

        public bool IsConnected()
        {
            return _client != null && _client.Connected;
        }

        public PLCConnection GetConnection()
        {
            return _connection;
        }

        /// <summary>
        /// Write data to PLC data block
        /// </summary>
        public bool WriteData(int dbNumber, int offset, byte[] data)
        {
            try
            {
                if (_networkStream == null || !IsConnected())
                    throw new InvalidOperationException("PLC not connected");

                // COTP and S7 protocol implementation would go here
                // This is a placeholder for the actual ISO-on-TCP protocol
                Console.WriteLine("Writing " + data.Length + " bytes to DB" + dbNumber + " at offset " + offset);
                
                // Send data (simplified - actual implementation requires ISO-on-TCP protocol)
                _networkStream.Write(data, 0, data.Length);
                _networkStream.Flush();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("PLC Write Error: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Read data from PLC data block
        /// </summary>
        public byte[] ReadData(int dbNumber, int offset, int count)
        {
            try
            {
                if (_networkStream == null || !IsConnected())
                    throw new InvalidOperationException("PLC not connected");

                byte[] data = new byte[count];
                
                // COTP and S7 protocol implementation would go here
                Console.WriteLine("Reading " + count + " bytes from DB" + dbNumber + " at offset " + offset);
                
                int bytesRead = _networkStream.Read(data, 0, count);
                return bytesRead > 0 ? data : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("PLC Read Error: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Send G-Code to PLC
        /// </summary>
        public bool WriteGCode(string gcode)
        {
            try
            {
                if (_networkStream == null || !IsConnected())
                    throw new InvalidOperationException("PLC not connected");

                byte[] data = Encoding.UTF8.GetBytes(gcode);
                return WriteData(1, 0, data);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error writing G-Code to PLC: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Get PLC diagnostics (placeholder for actual implementation)
        /// </summary>
        public string GetDiagnostics()
        {
            string status = IsConnected() ? "Connected" : "Disconnected";
            string ip = _connection != null ? _connection.IpAddress : "N/A";
            string rack = _connection != null ? _connection.Rack.ToString() : "N/A";
            string slot = _connection != null ? _connection.Slot.ToString() : "N/A";
            string connectedTime = _connection != null ? _connection.ConnectedDateTime.ToString() : "N/A";
            
            return "PLC Status: " + status + "\n" +
                   "Connection: " + ip + ":" + rack + ":" + slot + "\n" +
                   "Connected Since: " + connectedTime;
        }
    }
}
