using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace ApexSoft.MediatR
{
    /// <summary>
    /// ISender ve IPublisher'ın concrete implementasyonu.
    /// Handler ve pipeline behavior'ları DI container üzerinden çözümler.
    /// </summary>
    public class Sender(IServiceProvider provider) : IMediator
    {
        public async Task<TResponse> Send<TResponse>(
            IRequest<TResponse> request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            var requestType = request.GetType();
            var responseType = typeof(TResponse);

            // Handler'ı DI'dan çöz
            var handlerInterfaceType = typeof(IRequestHandler<,>).MakeGenericType(requestType, responseType);
            var handler = provider.GetService(handlerInterfaceType)
                ?? throw new InvalidOperationException(
                    $"'{requestType.FullName}' için handler bulunamadı. " +
                    $"'{handlerInterfaceType.FullName}' DI container'a register edilmemiş.");

            // Handler'ın Handle metodunu reflection ile al
            var handleMethod = handlerInterfaceType.GetMethod(nameof(IRequestHandler<IRequest<TResponse>, TResponse>.Handle))
                ?? throw new InvalidOperationException(
                    $"'{handlerInterfaceType.FullName}' üzerinde Handle metodu bulunamadı.");

            // Pipeline behavior'larını DI'dan çöz (kayıtlı sırayla)
            var behaviorInterfaceType = typeof(IPipelineBehavior<,>).MakeGenericType(requestType, responseType);
            var behaviors = provider.GetServices(behaviorInterfaceType)
                .Where(b => b is not null)
                .ToList();

            // En içteki next: handler.Handle(request, ct)
            Func<Task<TResponse>> next = () =>
                (Task<TResponse>)handleMethod.Invoke(handler, [request, cancellationToken])!;

            // Behavior'ları ters sırada zincirle (ilk register = en dışta çalışır)
            foreach (var behavior in Enumerable.Reverse(behaviors))
            {
                var capturedNext = next;
                var behaviorHandleMethod = behaviorInterfaceType.GetMethod(
                    nameof(IPipelineBehavior<IRequest<TResponse>, TResponse>.Handle))!;

                next = () =>
                    (Task<TResponse>)behaviorHandleMethod.Invoke(behavior, [request, cancellationToken, capturedNext])!;
            }

            return await next().ConfigureAwait(false);
        }

        public async Task Publish<TNotification>(
            TNotification notification,
            CancellationToken cancellationToken = default)
            where TNotification : INotification
        {
            ArgumentNullException.ThrowIfNull(notification);

            var notificationHandlerType = typeof(INotificationHandler<>).MakeGenericType(notification.GetType());
            var handlers = provider.GetServices(notificationHandlerType).ToList();

            if (handlers.Count == 0)
                return; // Notification handler'ı olmayan event'ler sessizce geçilir

            var handleMethod = notificationHandlerType.GetMethod(
                nameof(INotificationHandler<INotification>.Handle))!;

            // Tüm handler'ları paralel çalıştır
            var tasks = handlers.Select(handler =>
                (Task)handleMethod.Invoke(handler, [notification, cancellationToken])!);

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
    }
}
