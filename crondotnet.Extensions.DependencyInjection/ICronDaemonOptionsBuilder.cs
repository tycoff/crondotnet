using crondotnet;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    public interface ICronDaemonOptionsBuilder
    {
        ICronDaemonOptionsBuilder AddJob(IConfiguration configuration, string jobName, ExecuteCronJob job);

        ICronDaemonOptionsBuilder AddJob<TService>(IConfiguration configuration) where TService : IThinService;
    }
}