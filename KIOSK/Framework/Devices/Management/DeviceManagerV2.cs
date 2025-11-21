using KIOSK.Device.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KIOSK.Devices.Management
{
    public interface IDeviceManager : IAsyncDisposable
    {
        Task AddAsync(DeviceDescriptor desc, CancellationToken ct = default);

        // 상태
        event Action<string, DeviceStatusSnapshot>? StatusUpdated;
        IReadOnlyCollection<DeviceStatusSnapshot> GetLatestSnapshots();

        // 명령
        Task<CommandResult> SendAsync(string name, DeviceCommand cmd, CancellationToken ct = default);

        // 필요하면 헬퍼
        T? GetDevice<T>(string name) where T : class, IDevice;
    }

    public sealed class DeviceManagerV2 : IDeviceManager
    {
        private readonly IDeviceRuntime _runtime;
        private readonly IDeviceStatusStore _statusStore;
        private readonly IDeviceCommandBus _commandBus;

        public DeviceManagerV2(
            IDeviceRuntime runtime,
            IDeviceStatusStore statusStore,
            IDeviceCommandBus commandBus)
        {
            _runtime = runtime;
            _statusStore = statusStore;
            _commandBus = commandBus;

            _statusStore.StatusUpdated += (name, snap) => StatusUpdated?.Invoke(name, snap);
        }

        public Task AddAsync(DeviceDescriptor desc, CancellationToken ct = default)
            => _runtime.AddAsync(desc, ct);

        public event Action<string, DeviceStatusSnapshot>? StatusUpdated;

        public IReadOnlyCollection<DeviceStatusSnapshot> GetLatestSnapshots()
            => _statusStore.GetAll();

        public Task<CommandResult> SendAsync(string name, DeviceCommand cmd, CancellationToken ct = default)
            => _commandBus.SendAsync(name, cmd, ct);

        public T? GetDevice<T>(string name) where T : class, IDevice
        {
            if (_runtime.TryGetSupervisor(name, out var sup))
                return sup.Device as T;
            return null;
        }

        public ValueTask DisposeAsync() => _runtime.DisposeAsync();
    }
}
