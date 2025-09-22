using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace ApexSoft.MediatR
{
    public static class Mediator
    {
        public static IServiceCollection AddMediator(this IServiceCollection services, Assembly? assembly = null)
        {
            assembly ??= Assembly.GetCallingAssembly();

            services.AddScoped<ISender, Sender>();

            var handlerInterfaceType = typeof(IRequestHandler<,>);

            var handlerTypes = assembly
                .GetTypes()
                .Where(type => !type.IsAbstract && !type.IsInterface)
                .SelectMany(type => type.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterfaceType)
                    .Select(i => new { Interface = i, Implementation = type }));

            foreach (var handler in handlerTypes)
            {
                services.AddScoped(handler.Interface, handler.Implementation);
            }

            return services;
        }
    }
}