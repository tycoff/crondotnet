namespace crondotnet
{
    public interface ICronDaemonOptionsBuilder
    {
        ICronDaemonOptionsBuilder AddJob(ExecuteCronJob job, string expressionKey);

        ICronDaemonOptionsBuilder AddJob<TOptions>(ExecuteCronJob job, Func<TOptions, string> expressionSelector);

        ICronDaemonOptionsBuilder AddJob<TService>(string expressionKey) where TService : IThinService;

        ICronDaemonOptionsBuilder AddJob<TService, TOptions>(Func<TOptions, string> expressionSelector) where TService : IThinService;
    }
}