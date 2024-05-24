using crondotnet;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    public sealed class CronDaemonOptionsBuilder : ICronDaemonOptionsBuilder
    {
        private readonly List<Action<IServiceCollection>> Registrations = [];
        private IServiceCollection services;

        public CronDaemonOptionsBuilder(IServiceCollection services)
        {
            this.services = services;
        }

        public ICronDaemonOptionsBuilder AddJob(IConfiguration configuration, string jobName, ExecuteCronJob job)
        {
            Registrations.Add((services) =>
            {
                services.AddScoped<IInternalJob>(sp =>
                {
                    var options = sp.GetRequiredService<IOptionsMonitor<CronDaemonOptions>>();
                    return new InternalJob(Options.Options.Create(options.Get(jobName)), job);
                });

                services.Configure<CronDaemonOptions>(jobName, configuration);
            });

            return this;
        }

        public ICronDaemonOptionsBuilder AddJob<TService>(IConfiguration configuration)
            where TService : IThinService
        {
            Registrations.Add(services =>
            {
                services.AddScoped<IInternalJob, InternalJob<TService>>();
                services.Configure<CronDaemonOptions<TService>>(configuration);
            });

            services.TryAddScoped(typeof(TService));

            return this;
        }

        internal void Register(IServiceCollection services)
        {
            foreach (var registration in Registrations)
            {
                registration(services);
            }
        }
    }
}