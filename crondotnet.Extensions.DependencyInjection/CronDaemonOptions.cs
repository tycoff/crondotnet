using crondotnet;

namespace Microsoft.Extensions.DependencyInjection
{
    internal sealed class CronDaemonOptions
    {
        public string Expression { get; set; }
    }

    internal sealed class CronDaemonOptions<TService>
        where TService : IThinService
    {
        public string Expression { get; set; }
    }
}