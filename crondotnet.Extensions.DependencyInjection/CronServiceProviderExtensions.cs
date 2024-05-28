using Microsoft.Extensions.DependencyInjection;

namespace crondotnet
{
    public static class CronServiceProviderExtensions
    {
        public static IServiceCollection AddCron(this IServiceCollection services, Action<ICronDaemonOptionsBuilder>? cronBuilder = null)
        {
            services.AddHostedService<CronDaemonHostedService>();
            services.AddSingleton<ICronDaemon, CronDaemon>();

            var cronDaemonOptionsBuilder = new CronDaemonOptionsBuilder(services);
            cronBuilder?.Invoke(cronDaemonOptionsBuilder);

            cronDaemonOptionsBuilder.Register(services);

            return services;
        }
    }
}