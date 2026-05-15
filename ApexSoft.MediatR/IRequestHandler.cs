namespace ApexSoft.MediatR
{
    /// <summary>
    /// TResponse döndüren request handler'ları için interface.
    /// </summary>
    public interface IRequestHandler<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Response döndürmeyen (void) request handler'ları için kolaylık interface'i.
    /// Handle metodu Unit.Value döndürmelidir.
    /// </summary>
    public interface IRequestHandler<TRequest> : IRequestHandler<TRequest, Unit>
        where TRequest : IRequest<Unit>
    {
    }
}
