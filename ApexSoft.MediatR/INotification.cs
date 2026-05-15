namespace ApexSoft.MediatR
{
    /// <summary>
    /// Birden fazla handler'a yayımlanabilen event'ler için marker interface.
    /// </summary>
    public interface INotification { }

    /// <summary>
    /// Notification handler'ları için interface.
    /// Aynı notification için birden fazla handler register edilebilir.
    /// </summary>
    public interface INotificationHandler<TNotification>
        where TNotification : INotification
    {
        Task Handle(TNotification notification, CancellationToken cancellationToken);
    }
}
