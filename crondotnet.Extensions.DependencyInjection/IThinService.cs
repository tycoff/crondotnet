namespace crondotnet
{
    public interface IThinService
    {
        Task Run(CancellationToken cancellationToken);
    }
}
