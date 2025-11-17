// Core/DeviceManager.cs  (핵심 변경만)
using KIOSK.Device.Abstractions;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;

namespace KIOSK.Device.Core
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

           _snapshots[desc.Name] = new DeviceStatusSnapshot()
           {
               Name = desc.Name,
               Model = desc.Model,
           };

            sup.StatusUpdated += (id, snap) =>
            {
                _snapshots.AddOrUpdate(id, snap,
                    (_, prev) => snap.Timestamp >= prev.Timestamp ? snap : prev);
                StatusUpdated?.Invoke(id, snap);

                //Trace.WriteLine($"[{id}] {snap.Health} {string.Join(", ", snap.Alarms.Select(a => $"{a.Code}:{a.Message}"))}");
            };
            //sup.Connected += n => Connected?.Invoke(n);
            //sup.Faulted += (n, e) => Faulted?.Invoke(n, e);
            //sup.Disconnected += n => Disconnected?.Invoke(n);

            if (!_supers.TryAdd(desc.Name, sup))
                throw new InvalidOperationException($"Duplicated device name: {desc.Name}");

            _ = sup.RunAsync(CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, ct).Token);
            return Task.CompletedTask;
        }

        public Task<CommandResult> SendAsync(string name, DeviceCommand cmd, CancellationToken ct = default)
        {
            if (!_supers.TryGetValue(name, out var sup))
                return Task.FromResult(new CommandResult(false, $"Device not found: {name}"));
            return sup.ExecuteAsync(cmd, ct);
        }

        public IReadOnlyCollection<DeviceStatusSnapshot> GetLatestSnapshots()
        {
            return _snapshots.Values
                             .OrderBy(v => v.Name, StringComparer.OrdinalIgnoreCase)
                             .ToArray();
        }

        public T? GetDevice<T>(string name) where T : class, IDevice
        {
            if (_supers.TryGetValue(name, out var sup))
                return sup.Device as T;
            return null;
        }

        public IEnumerable<IDevice> GetAllDevices()
        {
            return _supers.Values
                .Select(s => s.Device)
                .Where(d => d != null)!;
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
