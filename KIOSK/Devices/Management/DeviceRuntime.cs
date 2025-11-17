using KIOSK.Device.Abstractions;
using KIOSK.Device.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KIOSK.Devices.Management
{
    public interface IDeviceRuntime : IAsyncDisposable
    {
        Task AddAsync(DeviceDescriptor desc, CancellationToken ct = default);

        bool TryGetSupervisor(string name, out DeviceSupervisor sup);
        IEnumerable<DeviceSupervisor> GetAllSupervisors();
    }

    public sealed class DeviceRuntime : IDeviceRuntime
    {
        private readonly ConcurrentDictionary<string, DeviceSupervisor> _supers = new();
        private readonly CancellationTokenSource _cts = new();
        private readonly IDeviceStatusStore _statusStore;

        public DeviceRuntime(IDeviceStatusStore statusStore)
        {
            _statusStore = statusStore;
        }

        public Task AddAsync(DeviceDescriptor desc, CancellationToken ct = default)
        {
            if (desc == null || desc.Validate == false)
                return Task.CompletedTask;

            var sup = new DeviceSupervisor(desc);

            // 상태 저장소에 초기 Offline 등록
            _statusStore.Initialize(desc);

            sup.StatusUpdated += (id, snap) =>
            {
                _statusStore.Update(id, snap);
            };

            if (!_supers.TryAdd(desc.Name, sup))
                throw new InvalidOperationException($"Duplicated device name: {desc.Name}");

            _ = sup.RunAsync(CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, ct).Token);
            return Task.CompletedTask;
        }

        public bool TryGetSupervisor(string name, out DeviceSupervisor sup)
            => _supers.TryGetValue(name, out sup);

        public IEnumerable<DeviceSupervisor> GetAllSupervisors() => _supers.Values;

        public async ValueTask DisposeAsync()
        {
            _cts.Cancel();
            foreach (var s in _supers.Values)
                await s.DisposeAsync();
            _cts.Dispose();
        }
    }
}
