using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace ApexSoft.MediatR
{
    public static class Mediator
    {
        public static IServiceCollection AddMediator(
            this IServiceCollection services,
            ServiceLifetime handlerLifetime = ServiceLifetime.Scoped,
            params Assembly[]? assemblies)
        {
            var assemblyList = new List<Assembly>();
            if (assemblies is null || assemblies.Length == 0)
            {
                var calling = Assembly.GetCallingAssembly();
                assemblyList.Add(calling);

                var entry = Assembly.GetEntryAssembly();
                if (entry != null && entry != calling)
                    assemblyList.Add(entry);
            }
            else
            {
                assemblyList.AddRange(assemblies.Distinct());
            }

            services.AddScoped<ISender, Sender>();

            var handlerInterfaceType = typeof(IRequestHandler<,>); // existing in your project
            var handlerRegistrations = assemblyList
                .SelectMany(a => SafeGetTypes(a))
                .Where(type => !type.IsAbstract && !type.IsInterface)
                .SelectMany(impl => impl.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterfaceType)
                    .Select(i => new { Interface = i, Implementation = impl }))
                .ToList();

            foreach (var reg in handlerRegistrations)
            {
                var descriptor = new ServiceDescriptor(reg.Interface, reg.Implementation, handlerLifetime);
                services.Add(descriptor);
            }
            var pipelineInterfaceName = "IPipelineBehavior";
            var pipelineTypes = assemblyList
                .SelectMany(a => SafeGetTypes(a))
                .Where(t => !t.IsAbstract && !t.IsInterface)
                .Select(t => new
                {
                    Type = t,
                    Behaviors = t.GetInterfaces()
                                 .Where(i => i.IsGenericType && i.GetGenericTypeDefinition().Name == pipelineInterfaceName)
                                 .ToArray()
                })
                .Where(x => x.Behaviors.Length > 0)
                .ToList();

            foreach (var p in pipelineTypes)
            {
                foreach (var iface in p.Behaviors)
                {
                    services.Add(new ServiceDescriptor(iface, p.Type, ServiceLifetime.Transient));
                }
            }

            return services;
        }
        private static IEnumerable<Type> SafeGetTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(t => t != null)!;
            }
            catch
            {
                return Enumerable.Empty<Type>();
            }
        }
    }
}
