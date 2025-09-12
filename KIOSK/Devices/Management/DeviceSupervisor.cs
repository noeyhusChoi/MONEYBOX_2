// Core/DeviceSupervisor.cs  (신규)
using Device.Abstractions;
using Device.Transport;

namespace Device.Core
{
    /// <summary>
    /// 장치 생명 주기 관리 -> 생명 주기 동안 연결/해제/상태 업데이트 처리
    /// </summary>
    public sealed class DeviceSupervisor : IAsyncDisposable
    {
        // TODO: 스냅샷 구조 변경이나 장치 연결까지 표시하는 방법 고려
        // TODO: Tranport 내부 I/O 직렬화 필요
        private readonly DeviceDescriptor _desc;
        private readonly SemaphoreSlim _gate = new(1, 1); // I/O 직렬화
        private ITransport? _transport;
        private IDevice? _device;

        public event Action<string>? Connected;
        public event Action<string>? Disconnected;
        public event Action<string, DeviceStatusSnapshot>? StatusUpdated;
        public event Action<string, Exception>? Faulted;

        public DeviceSupervisor(DeviceDescriptor desc)
        {
            _desc = desc;
        }

        public async Task RunAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    // 장치 연결
                    _transport = TransportFactory.Create(_desc);
                    _transport.Disconnected += (_, __) => Disconnected?.Invoke(_desc.Name);

                    _device = DeviceRegistry.Create(_desc, _transport);
                    await _transport.OpenAsync(ct).ConfigureAwait(false);
                    await _device.InitializeAsync(ct).ConfigureAwait(false);

                    using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct);
                    var pollMs = Math.Max(100, _desc.PollingMs);

                    // 상태 업데이트 루프
                    while (!linked.IsCancellationRequested)
                    {
                        try
                        {
                            if (_device is null) throw new InvalidOperationException("Device not ready");

                            await _gate.WaitAsync(linked.Token).ConfigureAwait(false);
                            try
                            {
                                var sn = await _device.GetStatusAsync(linked.Token, "").ConfigureAwait(false);

                                if (sn != null)
                                    StatusUpdated?.Invoke(_desc.Name, sn);
                            }
                            finally { _gate.Release(); }
                        }
                        catch (OperationCanceledException) { break; }
                        catch (Exception ex)
                        {
                            Faulted?.Invoke(_desc.Name, ex);
                            throw; // 상태 업데이트 루프 탈출
                        }

                        await Task.Delay(pollMs, linked.Token).ConfigureAwait(false);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {

                    var reconnectDelayMs = Math.Max(100, _desc.PollingMs);
                    await Task.Delay(reconnectDelayMs, ct).ConfigureAwait(false);
                }
                finally
                {
                    // 장치 연결 해제 및 정리
                    try { if (_transport is not null) await _transport.CloseAsync(ct).ConfigureAwait(false); } catch { }
                    try { await (_transport?.DisposeAsync() ?? ValueTask.CompletedTask); } catch { }
                }
            }
        }


        public async Task<CommandResult> ExecuteAsync(DeviceCommand cmd, CancellationToken ct = default)
        {
            if (_device is null) return new(false, "Device not connected");
            await _gate.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                return await _device.ExecuteAsync(cmd, ct).ConfigureAwait(false);
            }
            finally { _gate.Release(); }
        }

        public ValueTask DisposeAsync()
        {
            _gate.Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
