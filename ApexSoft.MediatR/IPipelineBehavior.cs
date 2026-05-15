namespace ApexSoft.MediatR
{
    /// <summary>
    /// Request pipeline'ına davranış eklemek için kullanılan interface.
    /// Logging, validation, caching gibi cross-cutting concern'ler için kullanılır.
    /// </summary>
    public interface IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        Task<TResponse> Handle(
            TRequest request,
            CancellationToken cancellationToken,
            Func<Task<TResponse>> next);
    }
}
