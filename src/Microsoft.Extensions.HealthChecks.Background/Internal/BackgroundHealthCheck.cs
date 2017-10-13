using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Extensions.HealthChecks.Background.Internal
{
    internal sealed class BackgroundHealthCheck : IHealthCheck, IDisposable
    {
        private readonly string _name;
        private readonly Func<CancellationToken, Task<IHealthCheckResult>> _checkFunc;
        private readonly TimeSpan _interval;
        private readonly TimeSpan? _delay;
        private IHealthCheckResult _latestResult;
        private readonly CancellationTokenSource _cancellation;
        private readonly Task _task;
        
        public BackgroundHealthCheck(string name, IHealthCheck check, TimeSpan interval, TimeSpan? delay = default)
            : this(name, _ => check.CheckAsync().AsTask(), interval, delay)
        {
        }
        

        public BackgroundHealthCheck(string name, Func<Task<IHealthCheckResult>> checkFunc, TimeSpan interval, TimeSpan? delay = default)
            : this(name, _ => checkFunc(), interval, delay)
        {
        }

        public BackgroundHealthCheck(string name, Func<CancellationToken, Task<IHealthCheckResult>> checkFunc, TimeSpan interval, TimeSpan? delay = default)
        {
            _name = name;
            _checkFunc = checkFunc;
            _interval = interval;
            _delay = delay;
            _latestResult = HealthCheckResult.Unknown($"{name}: Unknown");
            _cancellation = new CancellationTokenSource();
            _task = Start(_cancellation.Token);
        }

        private async Task Start(CancellationToken token)
        {
            if (_delay.HasValue && _delay.Value.TotalMilliseconds > 0)
            {
                await Task.Delay(_delay.Value, token);
            }
            while (!token.IsCancellationRequested)
            {
                var newResult = await _checkFunc(token);
                Interlocked.Exchange(ref _latestResult, newResult);
                if (token.IsCancellationRequested)
                {
                    break;
                }
                await Task.Delay(_interval, token);
            }
        }

        public ValueTask<IHealthCheckResult> CheckAsync(CancellationToken cancellationToken = default(CancellationToken))
            => new ValueTask<IHealthCheckResult>(_latestResult);

        public void Dispose()
        {
            _cancellation.Cancel(false);
            _cancellation.Dispose();
            _task.Dispose();
        }
    }
}