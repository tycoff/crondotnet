using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace crondotnet
{
    public sealed class CronDaemonOptionsBuilder : ICronDaemonOptionsBuilder
    {
        private readonly List<Action<IServiceCollection>> Registrations = [];
        private IServiceCollection services;

        public CronDaemonOptionsBuilder(IServiceCollection services)
        {
            this.services = services;
        }

        public ICronDaemonOptionsBuilder AddJob(ExecuteCronJob job, string expressionKey)
        {
            Registrations.Add((services) =>
            {
                services.AddSingleton<ICronJob>(sp =>
                {
                    return ActivatorUtilities.CreateInstance<InternalJob>(sp, [expressionKey, job]);
                });
            });

            return this;
        }

        public ICronDaemonOptionsBuilder AddJob<TService>(string expressionKey)
            where TService : IThinService
        {
            Registrations.Add(services =>
            {
                services.AddSingleton<ICronJob>(sp =>
                {
                    return ActivatorUtilities.CreateInstance<InternalJob<TService>>(sp, [expressionKey]);
                });
            });

            services.TryAddScoped(typeof(TService));

            return this;
        }

        public ICronDaemonOptionsBuilder AddJob<TOptions>(ExecuteCronJob job, Func<TOptions, string> expressionSelector)
        {
            Registrations.Add((services) =>
            {
                services.AddSingleton<ICronJob>(sp =>
                {
                    return ActivatorUtilities.CreateInstance<InternalJobWithOptions<TOptions>>(sp, [expressionSelector, job]);
                });
            });

            return this;
        }

        public ICronDaemonOptionsBuilder AddJob<TService, TOptions>(Func<TOptions, string> expressionSelector) where TService : IThinService
        {
            Registrations.Add(services =>
            {
                services.AddSingleton<ICronJob>(sp =>
                {
                    return ActivatorUtilities.CreateInstance<InternalJobWithOptions<TService, TOptions>>(sp, [expressionSelector]);
                });
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