using Microsoft.Extensions.DependencyInjection;

namespace ApexSoft.MediatR
{
    public class Sender(IServiceProvider provider) : ISender
    {        
        public Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            var sp = provider;
            var interfaceType = typeof(IRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResponse));
            var pipelineType = typeof(IPipelineBehavior<,>).MakeGenericType(request.GetType(), typeof(TResponse));

            RequestHandlerDelegate<TResponse> handlerDelete = () =>
            {
                var handler = sp.GetRequiredService(interfaceType);
                var method = interfaceType.GetMethod("Handle")!;
                return (Task<TResponse>)method.Invoke(handler, new object[] { request, cancellationToken })!;
            };

            var behaviors = (IEnumerable<object>)sp.GetServices(pipelineType);

            var pipeline = behaviors
                .Reverse()
                .Aggregate(
                    handlerDelete,
                    (next, behavior) =>
                    {
                        return () =>
                        {
                            var method = pipelineType.GetMethod("Handle")!;
                            return (Task<TResponse>)method.Invoke(
                                behavior,
                                new object[] { request, next, cancellationToken })!;
                        };
                    }
                );

            return pipeline();
        }
    }
}
