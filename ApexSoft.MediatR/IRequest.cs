namespace ApexSoft.MediatR
{
    /// <summary>
    /// TResponse döndüren request'ler için marker interface.
    /// </summary>
    public interface IRequest<TResponse> { }

    /// <summary>
    /// Response döndürmeyen (void) request'ler için. Unit döndürür.
    /// </summary>
    public interface IRequest : IRequest<Unit> { }
}
