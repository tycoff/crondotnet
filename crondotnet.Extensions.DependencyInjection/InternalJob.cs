using crondotnet;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    internal sealed class InternalJob : IInternalJob
    {
        public InternalJob(
                    IOptions<CronDaemonOptions> options,
                    ExecuteCronJob job)
        {
            Options = options;
            Job = job;
        }

        private IOptions<CronDaemonOptions> Options { get; }

        private ExecuteCronJob Job { get; }

        public string Expression => Options.Value.Expression;

        public Task ExecuteCronJob(CancellationToken cancellationToken)
        {
            return Job(cancellationToken);
        }
    }
}