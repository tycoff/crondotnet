using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;

namespace crondotnet
{
    internal sealed class InternalJobWithOptions<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TOptions> : ICronJob
    {
        [ActivatorUtilitiesConstructor]
        public InternalJobWithOptions(
                IOptionsMonitor<TOptions> options,
                Func<TOptions, string> expressionSelector,
                ExecuteCronJob job)
        {
            Schedule = new CronSchedule(expressionSelector(options.CurrentValue));
            options.OnChange((updatedOptions, key) =>
            {
                Schedule = new CronSchedule(expressionSelector(updatedOptions));
            });
            Job = job;
        }

        private ExecuteCronJob Job { get; }

        private ICronSchedule Schedule { get; set; }

        private SemaphoreSlim Semaphore { get; } = new(1);

        public Task ExecuteCronJob(CancellationToken cancellationToken)
        {
            return Job(cancellationToken);
        }

        public async Task Execute(DateTime startTime, CancellationToken cancellationToken)
        {
            try
            {
                await Semaphore.WaitAsync(cancellationToken);

                if (!Schedule.IsTime(startTime))
                    return;

                await Job(cancellationToken);
            }
            finally
            {
                Semaphore.Release();
            }
        }
    }
}