using Device.Abstractions;
using Pr22;
using System.Diagnostics;

namespace Device.Transport
{
    internal class TransportPr22 : ITransport
    {
        //DocumentReaderDevice pr;

        public event EventHandler? Disconnected;

        public TransportPr22()
        {
            //pr = new DocumentReaderDevice();
        }

        public bool IsOpen => false; // 실제 구현 필요

        public Task OpenAsync(CancellationToken ct = default)
        {
            //try 
            //{ 
            //    pr.UseDevice(0);
            //    pr.Scanner.StartTask(Pr22.Task.FreerunTask.Detection());
            //}
            //catch (Pr22.Exceptions.NoSuchDevice)
            //{
            //    Debug.WriteLine("Pr22 No device Found!");
            //}

            return Task.CompletedTask;
        }
        public Task CloseAsync(CancellationToken ct = default)
        {
            //try
            //{
            //    if (pr != null)
            //    {
            //        pr.Close();
            //    }
            //}
            //catch (Pr22.Exceptions.NoSuchDevice)
            //{
            //    Debug.WriteLine("Pr22 No device Found!");
            //}
            //catch (Exception ex)
            //{
            //    Debug.WriteLine($"Pr22 Close Error: {ex.Message}");
            //}

            return Task.CompletedTask;
        }

        // DLL 장치는 Read/Write 사용 안 함
        public Task<int> ReadAsync(Memory<byte> buffer, CancellationToken ct = default)
            => throw new NotSupportedException("DLL device doesn't use ReadAsync.");

        public Task WriteAsync(ReadOnlyMemory<byte> data, CancellationToken ct = default)
            => throw new NotSupportedException("DLL device doesn't use WriteAsync.");

        public ValueTask DisposeAsync()
        {
            //try { pr?.Dispose(); } catch { }
            return ValueTask.CompletedTask;
        }
    }
}
