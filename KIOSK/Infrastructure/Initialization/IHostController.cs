using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace KIOSK.Infrastructure.Initialization
{
    public interface IHostController
    {
        void AttachHost(IHost host);
        bool IsStarted { get; }
        Task StartAsync(CancellationToken ct = default);
        Task StopAsync(CancellationToken ct = default);
    }
}
