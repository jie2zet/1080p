using System.IO.Ports;
using System.Text;
using CMP.Services.Log;

namespace CMP.Services.Rfid;

/// <summary>
/// V640 1:1 串口客户端：按 CR 收发，同一端口串行化。
/// </summary>
public sealed class RfidSerialClient : IDisposable
{
    private readonly SerialPort _port;
    private readonly object _ioLock = new();
    private readonly bool _appendLf;
    private bool _disposed;

    public RfidSerialClient(string portName, RfidConfig config)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(portName);
        ArgumentNullException.ThrowIfNull(config);

        PortName = portName;
        _appendLf = config.AppendLf;
        _port = new SerialPort
        {
            PortName = portName,
            BaudRate = config.BaudRate,
            DataBits = config.DataBits,
            Parity = config.SerialParity,
            StopBits = config.SerialStopBits,
            Encoding = Encoding.ASCII,
            NewLine = _appendLf ? "\r\n" : "\r",
            Handshake = Handshake.None,
            ReadTimeout = config.ReadTimeoutMs,
            WriteTimeout = Math.Max(1000, config.ReadTimeoutMs),
            DtrEnable = true,
            RtsEnable = true
        };
    }

    public string PortName { get; }

    public bool IsOpen => _port.IsOpen;

    public void Open()
    {
        lock (_ioLock)
        {
            EnsureNotDisposed();
            if (_port.IsOpen)
            {
                return;
            }

            _port.Open();
            _port.DiscardInBuffer();
            _port.DiscardOutBuffer();
        }
    }

    public string Transact(string commandWithoutCr, int timeoutMs)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(commandWithoutCr);

        lock (_ioLock)
        {
            EnsureNotDisposed();
            if (!_port.IsOpen)
            {
                throw new InvalidOperationException($"RFID 串口未打开: {PortName}");
            }

            _port.DiscardInBuffer();
            _port.DiscardOutBuffer();
            _port.ReadTimeout = timeoutMs;
            _port.Write(commandWithoutCr + (_appendLf ? "\r\n" : "\r"));
            return ReadUntilCr(timeoutMs);
        }
    }

    public void Close()
    {
        lock (_ioLock)
        {
            if (_port.IsOpen)
            {
                try
                {
                    _port.Close();
                }
                catch (Exception ex)
                {
                    Logger.Warn($"关闭 RFID 串口 {PortName} 失败: {ex.Message}");
                }
            }
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        Close();
        _port.Dispose();
        _disposed = true;
    }

    private string ReadUntilCr(int timeoutMs)
    {
        var builder = new StringBuilder(64);
        var deadline = DateTime.UtcNow.AddMilliseconds(Math.Max(1, timeoutMs));

        while (DateTime.UtcNow < deadline)
        {
            try
            {
                var value = _port.ReadByte();
                if (value < 0)
                {
                    break;
                }

                if (value == 0x0D)
                {
                    return builder.ToString();
                }

                if (value != 0x0A)
                {
                    builder.Append((char)value);
                }
            }
            catch (TimeoutException)
            {
                break;
            }
        }

        if (builder.Length == 0)
        {
            throw new TimeoutException($"RFID 串口 {PortName} 等待响应超时 ({timeoutMs} ms)。");
        }

        return builder.ToString();
    }

    private void EnsureNotDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }
}
