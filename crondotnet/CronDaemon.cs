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

    public class CronDaemon : ICronDaemon
    {
        private readonly PeriodicTimer timer;
        private readonly List<ICronJob> cronJobs = new List<ICronJob>();
        private CancellationTokenSource tokenSource = null;
        private Task startTask = null;
        private Task cleanupTask = null;
        private readonly HashSet<Task> runningTasks = new HashSet<Task>();

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
            if (startTask == null)
            {
                tokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                startTask = InternalStart(tokenSource.Token);
                cleanupTask = PurgeTaskList(tokenSource.Token);
            }

            return Task.CompletedTask;
        }

        public async Task StopAsync()
        {
            try
            {
                tokenSource.Cancel();
                await startTask;
                await cleanupTask;
            }
            catch (OperationCanceledException)
            {
                // no-op
            }
        }

        private async Task InternalStart(CancellationToken cancellationToken)
        {
            var currentTime = DateTime.Now;
            var targetTime = currentTime.Date.AddHours(currentTime.Hour).AddMinutes(currentTime.Minute + 1);
            // .AddSeconds(currentTime.Second + (30 - (currentTime.Second % 30))); // If we wanted to increase resolution, this would allow for specify second targetting.

            DateTime? lastRun = null;

            // wait until the next 30 second interval before starting to trigger.
            await Task.Delay(targetTime - currentTime);

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (!cancellationToken.IsCancellationRequested && (!lastRun.HasValue || DateTime.Now.Minute != lastRun.Value.Minute))
                    {
                        lastRun = DateTime.Now;
                        foreach (ICronJob job in cronJobs)
                            runningTasks.Add(job.Execute(lastRun.Value, cancellationToken));
                    }

                    await timer.WaitForNextTickAsync(cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }

        private async Task PurgeTaskList(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (runningTasks.Count > 0)
                    {
                        var taskTask = await Task.WhenAny(runningTasks);
                        if (taskTask != null)
                            runningTasks.Remove(taskTask);
                    }
                    await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    // token was canceled.
                    break;
                }
            }

            // wait for remaining tasks to clean up
            await Task.WhenAll(runningTasks);
        }
    }
}
