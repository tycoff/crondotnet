using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;

namespace crondotnet
{
    internal sealed class InternalJobWithOptions<TService, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TOptions> : ICronJob
            where TService : IThinService
    {
        [ActivatorUtilitiesConstructor]
        public InternalJobWithOptions(
            IOptionsMonitor<TOptions> options,
            IServiceScopeFactory scopeFactory,
            Func<TOptions, string> expressionSelector)
        {
            Schedule = new CronSchedule(expressionSelector(options.CurrentValue));
            options.OnChange((updatedOptions, key) =>
            {
                Schedule = new CronSchedule(expressionSelector(updatedOptions));
            });
            ScopeFactory = scopeFactory;
        }

        private IServiceScopeFactory ScopeFactory { get; }

        private ICronSchedule Schedule { get; set; }

        private SemaphoreSlim Semaphore { get; } = new(1);

        public async Task Execute(DateTime startTime, CancellationToken cancellationToken)
        {
            try
            {
                await Semaphore.WaitAsync(cancellationToken);

                if (!Schedule.IsTime(startTime))
                    return;

                using var scope = ScopeFactory.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<TService>();
                if (service != null)
                {
                    await service.Run(cancellationToken);
                }
            }
            finally
            {
                Semaphore.Release();
            }
        }
    }
}