using KIOSK.Device.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KIOSK.Devices.Management
{
    public interface IDeviceCommandBus
    {
        Task<CommandResult> SendAsync(string name, DeviceCommand cmd, CancellationToken ct = default);
    }

    public sealed class DeviceCommandBus : IDeviceCommandBus
    {
        private readonly IDeviceRuntime _runtime;

        public DeviceCommandBus(IDeviceRuntime runtime)
        {
            _runtime = runtime;
        }

        public Task<CommandResult> SendAsync(string name, DeviceCommand cmd, CancellationToken ct = default)
        {
            if (!_runtime.TryGetSupervisor(name, out var sup))
                return Task.FromResult(new CommandResult(false, $"Device not found: {name}"));

            // 여기에서 나중에 로깅/타임아웃/리트라이 등 정책 넣기도 좋음
            return sup.ExecuteAsync(cmd, ct);
        }
    }
}
