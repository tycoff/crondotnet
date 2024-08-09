using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace crondotnet
{
    internal sealed class CronDaemonHostedService : BackgroundService
    {
        public CronDaemonHostedService(
                ILogger<CronDaemonHostedService> logger,
                ICronDaemon cronDaemon,
                IEnumerable<ICronJob> jobs)
        {
            Logger = logger;
            CronDaemon = cronDaemon;
            Jobs = jobs;
        }

        private ILogger<CronDaemonHostedService> Logger { get; }

        private ICronDaemon CronDaemon { get; set; }

        private IEnumerable<ICronJob> Jobs { get; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                foreach (var job in Jobs)
                {
                    CronDaemon.AddJob(job);
                }

                await CronDaemon.StartAsync(stoppingToken);

                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (Exception ex) when (ex is OperationCanceledException || ex is TaskCanceledException) { }
            catch (Exception ex)
            {
                Logger.LogError(ex, "An unexpected exception occurred while starting the cron daemon.");
                throw;
            }
            finally
            {
                await CronDaemon.StopAsync();
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            CronDaemon?.Dispose();
        }
    }
}