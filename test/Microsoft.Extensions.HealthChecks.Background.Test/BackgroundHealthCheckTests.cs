using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Extensions.HealthChecks.Background.Test
{
    public class BackgroundHealthCheckTests
    {
        [Fact]
        public async Task FuncRunsInBackground()
        {
            var builder = new HealthCheckBuilder();
            int count = 0;
            builder.AddBackgroundCheck("test", () =>
            {
                Interlocked.Increment(ref count);
                return Task.FromResult<IHealthCheckResult>(HealthCheckResult.Healthy("test"));
            }, TimeSpan.FromMilliseconds(10));
            await Task.Delay(100);
            Assert.True(count > 0);
        }

        [Fact]
        public async Task TypeRunsInBackground()
        {
            var builder = new HealthCheckBuilder();
            var check = new TypeHealthCheck();
            builder.AddBackgroundCheck("test", check, TimeSpan.FromMilliseconds(10));
            await Task.Delay(100);
            Assert.True(check.Count > 0);
        }
        
        [Fact]
        public async Task FuncDelays()
        {
            var builder = new HealthCheckBuilder();
            int count = 0;
            builder.AddBackgroundCheck("test", () =>
            {
                Interlocked.Increment(ref count);
                return Task.FromResult<IHealthCheckResult>(HealthCheckResult.Healthy("test"));
            }, TimeSpan.FromMilliseconds(10), TimeSpan.FromMinutes(1));
            await Task.Delay(100);
            Assert.Equal(0, count);
        }

        [Fact]
        public async Task TypeDelays()
        {
            var builder = new HealthCheckBuilder();
            var check = new TypeHealthCheck();
            builder.AddBackgroundCheck("test", check, TimeSpan.FromMilliseconds(10), TimeSpan.FromMinutes(1));
            await Task.Delay(100);
            Assert.Equal(0, check.Count);
        }
        
        private class TypeHealthCheck : IHealthCheck
        {
            private int _count;

            public int Count => _count;

            public ValueTask<IHealthCheckResult> CheckAsync(CancellationToken cancellationToken = default(CancellationToken))
            {
                Interlocked.Increment(ref _count);
                return new ValueTask<IHealthCheckResult>(HealthCheckResult.Healthy("test"));
            }
        }
    }
}