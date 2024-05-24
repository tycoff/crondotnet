using crondotnet;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    internal sealed class InternalJob<TService> : IInternalJob
            where TService : IThinService
    {
        public InternalJob(
                IOptionsMonitor<CronDaemonOptions<TService>> options,
                IServiceScopeFactory scopeFactory)
        {
            Options = options;
            ScopeFactory = scopeFactory;
        }

        public string Expression => this.Options.CurrentValue.Expression;

        private IOptionsMonitor<CronDaemonOptions<TService>> Options { get; }

        private IServiceScopeFactory ScopeFactory { get; }

        public async Task ExecuteCronJob(CancellationToken cancellationToken)
        {
            using var scope = ScopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<TService>();
            if (service != null)
            {
                await service.Run(cancellationToken);
            }
        }
    }
}