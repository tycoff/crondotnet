namespace Microsoft.Extensions.DependencyInjection
{
    internal interface IInternalJob
    {
        string Expression { get; }
        Task ExecuteCronJob(CancellationToken cancellationToken);
    }
}