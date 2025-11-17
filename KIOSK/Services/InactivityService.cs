using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace KIOSK.Services
{
    public interface IInactivityService
    {
        void Start(TimeSpan timeout, Action onTimeout);
        void Reset();
        void Stop();
    }

    public class InactivityService : IInactivityService, IDisposable
    {
        private readonly DispatcherTimer _timer;
        private Action? _onTimeout;

        public InactivityService()
        {
            _timer = new DispatcherTimer();
            _timer.Tick += OnTick;
        }

        public void Start(TimeSpan timeout, Action onTimeout)
        {
            _onTimeout = onTimeout;
            _timer.Interval = timeout;

            _timer.Stop();
            _timer.Start();
        }

        public void Reset()
        {
            if (!_timer.IsEnabled) 
                return;

            _timer.Stop();
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
            _onTimeout = null;
        }

        private void OnTick(object? sender, EventArgs e)
        {
            _timer.Stop();
            _onTimeout?.Invoke();
        }

        public void Dispose()
        {
            _timer.Tick -= OnTick;
            _timer.Stop();
        }
    }
}
