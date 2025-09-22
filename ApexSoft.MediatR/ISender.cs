using Microsoft.Extensions.DependencyInjection;

namespace ApexSoft.MediatR
{
    public interface ISender
    {
        Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
    }
}