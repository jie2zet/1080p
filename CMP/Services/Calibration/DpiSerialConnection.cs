using System.Diagnostics;
using System.Globalization;
using System.IO.Ports;
using System.Text;

namespace CMP.Services.Calibration
{
    /// <summary>
    /// DPI 压力计串口连接：每轮自动校准独占一个连接，负责发送 ":sens?" 并解析 ":SENS:PRES" 响应。
    /// </summary>
    public sealed class DpiSerialConnection : IDisposable
    {
        private const string QueryCommand = ":sens?\r\n";
        private const string ResponsePrefix = ":SENS:PRES";
        private const int ResponseTimeoutMs = 2000;
        private const int ReceiveIdleMs = 150;
        private const int MaxResponseLength = 1024;

        private readonly SerialPort _port;

        public DpiSerialConnection(string portName, int baudRate)
        {
            _port = new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One)
            {
                Encoding = Encoding.ASCII,
                ReadTimeout = ResponseTimeoutMs,
                WriteTimeout = ResponseTimeoutMs
            };
        }

        public void Open() => _port.Open();

        /// <summary>
        /// 发送一次压力查询并返回 DPI 读数。
        /// 收到回车/换行或接收空闲 150ms 后解析；整体超时 2 秒。
        /// </summary>
        public async Task<float> ReadPressureAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _port.DiscardInBuffer();
            _port.Write(QueryCommand);

            var response = new StringBuilder();
            var elapsed = Stopwatch.StartNew();
            long lastReceived = 0;

            while (elapsed.ElapsedMilliseconds < ResponseTimeoutMs)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var chunk = _port.ReadExisting();
                if (chunk.Length > 0)
                {
                    response.Append(chunk);
                    lastReceived = elapsed.ElapsedMilliseconds;
                    if (response.Length > MaxResponseLength)
                        throw new FormatException("DPI response too long.");
                }

                // 收到回车或换行符时解析响应；无结束符时等待接收空闲后再解析。
                if (response.Length > 0 && (chunk.Contains('\r') || chunk.Contains('\n') ||
                    elapsed.ElapsedMilliseconds - lastReceived >= ReceiveIdleMs))
                {
                    return ParsePressure(response.ToString());
                }

                await Task.Delay(20, cancellationToken).ConfigureAwait(false);
            }

            throw new TimeoutException($"{_port.PortName}: DPI response timed out.");
        }

        /// <summary>
        /// 解析 DPI 响应，例如 ":SENS:PRES 12.345"。
        /// </summary>
        public static float ParsePressure(string response)
        {
            var text = response.Trim();
            if (!text.StartsWith(ResponsePrefix, StringComparison.OrdinalIgnoreCase) ||
                text.Length <= ResponsePrefix.Length ||
                !char.IsWhiteSpace(text[ResponsePrefix.Length]) ||
                !float.TryParse(text[ResponsePrefix.Length..].Trim(), NumberStyles.Float,
                    CultureInfo.InvariantCulture, out var value) ||
                !float.IsFinite(value))
            {
                throw new FormatException($"Invalid DPI response: {response}");
            }

            return value;
        }

        public void Dispose() => _port.Dispose();
    }
}
