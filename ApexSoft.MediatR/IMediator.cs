namespace ApexSoft.MediatR
{
    /// <summary>
    /// Request göndermek için kullanılan interface.
    /// </summary>
    public interface ISender
    {
        Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Notification yayımlamak için kullanılan interface.
    /// </summary>
    public interface IPublisher
    {
        Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotification;
    }

    /// <summary>
    /// Hem Send hem Publish işlemlerini kapsayan ana mediator interface'i.
    /// </summary>
    public interface IMediator : ISender, IPublisher { }
}
