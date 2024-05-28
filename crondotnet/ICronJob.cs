namespace crondotnet
{
    public delegate Task ExecuteCronJob(CancellationToken cancellationToken);

    public interface ICronJob
    {
        Task Execute(DateTime startTime, CancellationToken cancellationToken);
    }
}
