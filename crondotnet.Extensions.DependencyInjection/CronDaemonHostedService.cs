using Microsoft.Extensions.Hosting;

namespace crondotnet
{
    internal sealed class CronDaemonHostedService : IHostedService
    {
        public CronDaemonHostedService(
                ICronDaemon cronDaemon,
                IEnumerable<ICronJob> jobs)
        {
            CronDaemon = cronDaemon;
            Jobs = jobs;
        }

        private ICronDaemon CronDaemon { get; set; }

        private IEnumerable<ICronJob> Jobs { get; }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            foreach (var job in Jobs)
            {
                CronDaemon.AddJob(job);
            }

            await CronDaemon.StartAsync(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await CronDaemon.StopAsync();
        }
    }
}