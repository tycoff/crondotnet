namespace crondotnet
{
    public interface ICronDaemon
    {
        void AddJob(ICronJob cronJob);
        Task StartAsync(CancellationToken cancellationToken);
        Task StopAsync();
    }
}
