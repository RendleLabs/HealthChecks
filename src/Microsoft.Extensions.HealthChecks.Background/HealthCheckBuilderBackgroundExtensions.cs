using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.HealthChecks.Background.Internal;

namespace Microsoft.Extensions.HealthChecks.Background
{
    public static class HealthCheckBuilderBackgroundExtensions
    {
        public static HealthCheckBuilder AddBackgroundCheck(this HealthCheckBuilder builder, string name,
            Func<Task<IHealthCheckResult>> checkFunc, TimeSpan interval, TimeSpan? delay = default) =>
            builder.AddCheck(name, new BackgroundHealthCheck(name, checkFunc, interval, delay));
        
        public static HealthCheckBuilder AddBackgroundCheck(this HealthCheckBuilder builder, string name,
            Func<CancellationToken, Task<IHealthCheckResult>> checkFunc, TimeSpan interval, TimeSpan? delay = default) =>
            builder.AddCheck(name, new BackgroundHealthCheck(name, checkFunc, interval, delay));
        
        public static HealthCheckBuilder AddBackgroundCheck(this HealthCheckBuilder builder, string name,
            IHealthCheck check, TimeSpan interval, TimeSpan? delay = default) =>
            builder.AddCheck(name, new BackgroundHealthCheck(name, check, interval, delay));
    }
}