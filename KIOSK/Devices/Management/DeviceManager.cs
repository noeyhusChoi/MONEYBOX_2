// Core/DeviceManager.cs  (핵심 변경만)
using Device.Abstractions;
using Device.Devices;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Device.Core
{
    public sealed class DeviceManager : IAsyncDisposable
    {
        private readonly ConcurrentDictionary<string, DeviceSupervisor> _supers = new();
        private readonly CancellationTokenSource _cts = new();
        private readonly ConcurrentDictionary<string, DeviceStatusSnapshot> _snapshots = new();

        public event Action<string, DeviceStatusSnapshot>? StatusUpdated;
        public event Action<string>? Connected;
        public event Action<string, Exception>? Faulted;
        public event Action<string>? Disconnected;

        public Task AddAsync(DeviceDescriptor desc, CancellationToken ct = default)
        {
            if (desc == null || desc.Validate == false)
                return Task.CompletedTask;

            var sup = new DeviceSupervisor(desc);
            sup.StatusUpdated += (n, s) => StatusUpdated?.Invoke(n, s);
            sup.StatusUpdated += (id, snap) =>
            {
                // 분리형 클래스 사용 시: _snapshots.Upsert(snap);
                _snapshots.AddOrUpdate(id, snap,
                    (_, prev) => snap.Timestamp >= prev.Timestamp ? snap : prev);

                StatusUpdated?.Invoke(id, snap);
            };
            sup.Connected += n => Connected?.Invoke(n);
            sup.Faulted += (n, e) => Faulted?.Invoke(n, e);
            sup.Disconnected += n => Disconnected?.Invoke(n);

            if (!_supers.TryAdd(desc.Name, sup))
                throw new InvalidOperationException($"Duplicated device name: {desc.Name}");

            // fire-and-forget 실행 → 각 장치는 독립적으로 자동 재연결
            _ = sup.RunAsync(CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, ct).Token);
            return Task.CompletedTask;
        }

        public Task<CommandResult> SendAsync(string name, DeviceCommand cmd, CancellationToken ct = default)
        {
            if (!_supers.TryGetValue(name, out var sup))
                return Task.FromResult(new CommandResult(false, $"Device not found: {name}"));
            return sup.ExecuteAsync(cmd, ct);
        }

        /// <summary>
        /// 현재 보관 중인 모든 장치의 최신 스냅샷
        /// </summary>
        public IReadOnlyCollection<DeviceStatusSnapshot> GetLatestSnapshots()
        {
            // 분리형 클래스 사용 시: return _snapshots.GetAll();
            return _snapshots.Values
                             .OrderBy(v => v.Name, StringComparer.OrdinalIgnoreCase)
                             .ToArray();
        }

        public async ValueTask DisposeAsync()
        {
            _cts.Cancel();
            foreach (var s in _supers.Values)
                await s.DisposeAsync();
            _cts.Dispose();
        }
    }
}