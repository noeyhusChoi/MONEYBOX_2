// Transport/SerialTransport.cs
using Devices.Abstractions;
using System.IO.Ports;
using System.Runtime.InteropServices;

namespace Devices.Transport
{
    public sealed class TransportSerial : ITransport
    {
        private readonly SerialPort _port;

        public event EventHandler? Disconnected;

        public TransportSerial(string portName, int baudRate,
            int dataBits = 8, StopBits stopBits = StopBits.One, Parity parity = Parity.None)
        {
            _port = new SerialPort(portName, baudRate, parity, dataBits, stopBits)
            {
                ReadTimeout = 500,
                WriteTimeout = 500
            };
        }

        public bool IsOpen => _port.IsOpen;

        public Task OpenAsync(CancellationToken ct = default)
        {
            try 
            {
                if (!_port.IsOpen)
                {
                    _port.Open(); 
                }
            }
            catch { throw; }

            return Task.CompletedTask;
        }

        public Task CloseAsync(CancellationToken ct = default)
        {
            try
            {
                if (_port.IsOpen)
                {
                    _port.Close();
                    Disconnected?.Invoke(this, EventArgs.Empty);
                }
            }
            catch { }

            return Task.CompletedTask;
        }

        public async Task<int> ReadAsync(Memory<byte> buffer, CancellationToken ct = default)
        {
            try
            {
                return await Task.Run(() =>
                {
                    // 바이트 배열만 지원
                    if (!MemoryMarshal.TryGetArray(buffer, out ArraySegment<byte> seg) || seg.Array is null)
                        throw new ArgumentException("Buffer must be array-backed.", nameof(buffer));

                    try
                    {
                        return _port.Read(seg.Array, seg.Offset, seg.Count);
                    }
                    catch (TimeoutException)
                    {
                        // 시간 초과
                        return 0;
                    }
                }, ct).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // 정상 종료
                return 0;
            }
            catch
            {
                throw;
            }
        }

        public async Task WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken ct = default)
        {
            try
            {
                await Task.Run(() =>
                {
                    // 바이트 배열만 지원
                    if (!MemoryMarshal.TryGetArray(buffer, out ArraySegment<byte> seg) || seg.Array is null)
                        throw new ArgumentException("Buffer must be array-backed.", nameof(buffer));

                    try
                    {
                        _port.Write(seg.Array, seg.Offset, seg.Count);
                    }
                    catch (TimeoutException)
                    {
                        // 시간 초과
                    }
                }, ct).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // 정상 종료
            }
            catch
            {
                throw;
            }
        }

        public ValueTask DisposeAsync()
        {
            try { _port.Dispose(); } catch { }
            return ValueTask.CompletedTask;
        }
    }
}
