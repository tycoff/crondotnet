using DotNext.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace crondotnet
{
    public interface ICronDaemon
    {
        void AddJob(string schedule, ExecuteCronJob action);
        Task StartAsync(CancellationToken cancellationToken);
        Task StopAsync();
    }

    internal sealed class CronDaemon : ICronDaemon
    {
        private readonly PeriodicTimer timer;
        private readonly List<ICronJob> cronJobs = [];
        private CancellationTokenSource tokenSource = null;
        private Task startTask = null;
        private readonly TaskCompletionPipe<Task> taskCompletionPipe = new TaskCompletionPipe<Task>();

        public CronDaemon()
        {
            timer = new PeriodicTimer(TimeSpan.FromSeconds(30));
        }

        public void AddJob(string schedule, ExecuteCronJob action)
        {
            var cj = new CronJob(schedule, action);
            cronJobs.Add(cj);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            tokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            startTask ??= InternalStart(tokenSource.Token);
            return startTask;
        }

        public Task StopAsync()
        {
            tokenSource.Cancel();
            return Task.CompletedTask;
        }

        private async Task InternalStart(CancellationToken cancellationToken)
        {
            var currentTime = DateTime.Now;
            var targetTime = currentTime.Date.AddHours(currentTime.Hour).AddMinutes(currentTime.Minute + 1);
            // .AddSeconds(currentTime.Second + (30 - (currentTime.Second % 30))); // If we wanted to increase resolution, this would allow for specify second targetting.

            DateTime? lastRun = null;

            // wait until the next 30 second interval before starting to trigger.
            await Task.Delay(targetTime - currentTime, cancellationToken);

            while (!cancellationToken.IsCancellationRequested)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!cancellationToken.IsCancellationRequested && (!lastRun.HasValue || DateTime.Now.Minute != lastRun.Value.Minute))
                {
                    lastRun = DateTime.Now;
                    foreach (ICronJob job in cronJobs)
                    {
                        taskCompletionPipe.Add(job.Execute(lastRun.Value, cancellationToken));
                    }
                }

                await timer.WaitForNextTickAsync(cancellationToken);
            }
        }
    }
}
