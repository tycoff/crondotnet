using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace crondotnet.Extensions.DependencyInjection
{
    internal sealed class CronDaemonHostedService : IHostedService
    {
        public CronDaemonHostedService(
                ICronDaemon cronDaemon,
                IEnumerable<IInternalJob> jobs)
        {
            CronDaemon = cronDaemon;
            Jobs = jobs;
        }

        private ICronDaemon CronDaemon { get; set; }

        private IEnumerable<IInternalJob> Jobs { get; }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            foreach (var job in Jobs)
            {
                CronDaemon.AddJob(job.Expression, job.ExecuteCronJob);
            }

            await CronDaemon.StartAsync(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await CronDaemon.StopAsync();
        }
    }
}