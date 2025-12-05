using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace KIOSK.Infrastructure.Initialization
{
    /// <summary>
    /// IHost 시작/정지를 제어하기 위한 컨트롤러. Host는 Build 이후 AttachHost로 주입한다.
    /// </summary>
    public sealed class HostController : IHostController
    {
        private readonly SemaphoreSlim _gate = new(1, 1);
        private IHost? _host;
        private bool _started;

        public bool IsStarted => _started;

        public void AttachHost(IHost host)
        {
            _host = host ?? throw new ArgumentNullException(nameof(host));
        }

        public async Task StartAsync(CancellationToken ct = default)
        {
            if (_started)
                return;

            await _gate.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                if (_started)
                    return;

                if (_host is null)
                    throw new InvalidOperationException("Host is not attached.");

                await _host.StartAsync(ct).ConfigureAwait(false);
                _started = true;
            }
            finally
            {
                _gate.Release();
            }
        }

        public async Task StopAsync(CancellationToken ct = default)
        {
            if (!_started)
                return;

            await _gate.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                if (!_started)
                    return;

                if (_host is null)
                    return;

                await _host.StopAsync(ct).ConfigureAwait(false);
                _started = false;
            }
            finally
            {
                _gate.Release();
            }
        }
    }
}
