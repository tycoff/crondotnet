namespace crondotnet
{
    public interface ICronDaemon : IDisposable
    {
        void AddJob(ICronJob cronJob);
        Task StartAsync(CancellationToken cancellationToken);
        Task StopAsync();
    }
}
