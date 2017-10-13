using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using StackExchange.Redis;

namespace Microsoft.Extensions.HealthChecks.Redis
{
    public static class HealthCheckBuilderRedisExtensions
    {
        public static HealthCheckBuilder AddRedisCheck(this HealthCheckBuilder builder, string name,
            string configuration, TimeSpan cacheDuration)
        {
            builder.AddCheck(name, async () =>
            {
                try
                {
                    var cn = await ConnectionMultiplexer.ConnectAsync(configuration);
                    var ping = await cn.GetDatabase().PingAsync();
                    return HealthCheckResult.Healthy($"RedisCheck({name}): Healthy",
                        new Dictionary<string, object>
                        {
                            ["ping"] = ping.ToString()
                        });
                }
                catch (Exception e)
                {
                    return HealthCheckResult.Unhealthy($"RedisCheck({name}): Exception: {e.GetType().FullName}",
                        new Dictionary<string, object>
                        {
                            ["exceptionType"] = e.GetType().FullName,
                            ["exceptionMessage"] = e.Message
                        });
                }
            }, cacheDuration);

            return builder;
        }
    }
}