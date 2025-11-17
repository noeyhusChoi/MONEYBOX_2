// Core/DeviceSupervisor.cs  (신규)
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KIOSK.Device.Abstractions;
using KIOSK.Device.Transport;

namespace KIOSK.Device.Core
{
    /// <summary>
    /// 장치 생명 주기 관리 -> 생명 주기 동안 연결/해제/상태 업데이트 처리
    /// </summary>
    public sealed class DeviceSupervisor : IAsyncDisposable
    {
        private readonly DeviceDescriptor _desc;
        private readonly SemaphoreSlim _gate = new(1, 1);
        private ITransport? _transport;
        private IDevice? _device;

        public event Action<string>? Connected;
        public event Action<string>? Disconnected;
        public event Action<string, DeviceStatusSnapshot>? StatusUpdated;
        public event Action<string, Exception>? Faulted;

        public IDevice? Device => _device;

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
                    _transport = TransportFactory.Create(_desc);
                    _transport.Disconnected += (_, __) => Disconnected?.Invoke(_desc.Name);

                    _device = DeviceRegistry.Create(_desc, _transport);
                    await _transport.OpenAsync(ct).ConfigureAwait(false);

                    var initSnapshot = await _device.InitializeAsync(ct).ConfigureAwait(false);
                    bool hasError = HasError(initSnapshot);

                    if (initSnapshot != null)
                        StatusUpdated?.Invoke(_desc.Name, initSnapshot);

                    if (!hasError)
                    {
                        Connected?.Invoke(_desc.Name);

                        using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct);
                        var pollMs = Math.Max(100, _desc.PollingMs);

                        while (!linked.IsCancellationRequested)
                        {
                            try
                            {
                                if (_device is null)
                                    throw new InvalidOperationException("Device not ready");

                                await _gate.WaitAsync(linked.Token).ConfigureAwait(false);
                                try
                                {
                                    var sn = await _device.GetStatusAsync(linked.Token, "").ConfigureAwait(false);
                                    if (sn == null) break;
                                    StatusUpdated?.Invoke(_desc.Name, sn);
                                }
                                finally
                                {
                                    _gate.Release();
                                }
                            }
                            catch (OperationCanceledException)
                            {
                                break;
                            }
                            catch (Exception ex)
                            {
                                Faulted?.Invoke(_desc.Name, ex);
                                throw;
                            }

                            await Task.Delay(pollMs, linked.Token).ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        var reconnectDelayMs = Math.Max(100, _desc.PollingMs);
                        await Task.Delay(reconnectDelayMs, ct).ConfigureAwait(false);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception)
                {
                    var reconnectDelayMs = Math.Max(100, _desc.PollingMs);
                    await Task.Delay(reconnectDelayMs, ct).ConfigureAwait(false);
                }
                finally
                {
                    try
                    {
                        if (_device is IAsyncDisposable asyncDisposable)
                        {
                            await asyncDisposable.DisposeAsync().ConfigureAwait(false);
                        }
                        else if (_device is IDisposable disposable)
                        {
                            disposable.Dispose();
                        }
                    }
                    catch
                    {
                    }
                    finally
                    {
                        _device = null;
                    }

                    try
                    {
                        if (_transport is not null)
                            await _transport.CloseAsync(ct).ConfigureAwait(false);
                    }
                    catch
                    {
                    }

                    try
                    {
                        await (_transport?.DisposeAsync() ?? ValueTask.CompletedTask);
                    }
                    catch
                    {
                    }
                    finally
                    {
                        _transport = null;
                    }
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
            finally
            {
                _gate.Release();
            }
        }

        public ValueTask DisposeAsync()
        {
            _gate.Dispose();
            return ValueTask.CompletedTask;
        }

        private static bool HasError(DeviceStatusSnapshot? snapshot)
        {
            if (snapshot is null)
                return false;

            if (snapshot.Alarms is null || snapshot.Alarms.Count == 0)
                return false;

            return snapshot.Alarms.Any(a => a.Severity is Severity.Error or Severity.Critical);
        }
    }
}
