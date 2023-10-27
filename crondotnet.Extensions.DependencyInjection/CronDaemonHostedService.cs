using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace crondotnet.Extensions.DependencyInjection
{
    internal class CronDaemonHostedService : IHostedService
    {
        public CronDaemonHostedService(
                IServiceScopeFactory scopeFactory,
                ICronDaemon cronDaemon,
                IOptions<CronDaemonOptions> options)
        {
            ScopeFactory = scopeFactory;
            CronDaemon = cronDaemon;
            Options = options;
        }

        private IServiceScopeFactory ScopeFactory { get; }

        private ICronDaemon CronDaemon { get; set; }

        private IOptions<CronDaemonOptions> Options { get; }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            foreach (var job in Options.Value.Jobs)
            {
                if (job.StaticTask != null)
                {
                    CronDaemon.AddJob(job.Expression, job.StaticTask);
                }
                else if (job.ServiceType != null)
                {
                    CronDaemon.AddJob(job.Expression, new ThinServiceCronWrapper(ScopeFactory, job.ServiceType).RunService);
                }
                else
                {
                    // nothing set up for the run task.
                }
            }

            await CronDaemon.StartAsync(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                await CronDaemon.StopAsync();
            }
            catch (OperationCanceledException)
            {
                // no-op
            }
        }

        private sealed class ThinServiceCronWrapper
        {
            public ThinServiceCronWrapper(
                        IServiceScopeFactory scopeFactory,
                        Type serviceType)
            {
                ScopeFactory = scopeFactory;
                ServiceType = serviceType;
            }

            private IServiceScopeFactory ScopeFactory { get; }
            private Type ServiceType { get; }


            public async Task RunService(CancellationToken cancellationToken)
            {
                using var scope = ScopeFactory.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService(ServiceType) as IThinService;
                if (service != null)
                {
                    await service.Run(cancellationToken);
                }
            }
        }
    }
}