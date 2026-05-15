using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace ApexSoft.MediatR
{
    /// <summary>
    /// ApexSoft.MediatR kütüphanesini DI container'a register etmek için extension metodları.
    /// </summary>
    public static class MediatorExtensions
    {
        /// <summary>
        /// Handler'ları, notification handler'ları ve pipeline behavior'ları otomatik olarak
        /// DI container'a register eder.
        /// </summary>
        /// <param name="services">DI service collection</param>
        /// <param name="handlerLifetime">Handler'ların lifetime'ı (varsayılan: Scoped)</param>
        /// <param name="assemblies">Taranacak assembly'ler. Boş bırakılırsa entry assembly taranır.</param>
        public static IServiceCollection AddMediator(
            this IServiceCollection services,
            ServiceLifetime handlerLifetime = ServiceLifetime.Scoped,
            params Assembly[]? assemblies)
        {
            var assemblyList = ResolveAssemblies(assemblies);

            // IMediator, ISender, IPublisher → Sender
            services.AddScoped<IMediator, Sender>();
            services.AddScoped<ISender>(sp => sp.GetRequiredService<IMediator>());
            services.AddScoped<IPublisher>(sp => sp.GetRequiredService<IMediator>());

            var allTypes = assemblyList
                .SelectMany(SafeGetTypes)
                .Where(t => t is { IsAbstract: false, IsInterface: false })
                .ToList();

            RegisterRequestHandlers(services, allTypes, handlerLifetime);
            RegisterNotificationHandlers(services, allTypes, handlerLifetime);
            RegisterPipelineBehaviors(services, allTypes);

            return services;
        }

        private static void RegisterRequestHandlers(
            IServiceCollection services,
            List<Type> types,
            ServiceLifetime lifetime)
        {
            var handlerInterface = typeof(IRequestHandler<,>);

            foreach (var impl in types)
            {
                var matchedInterfaces = impl.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterface);

                foreach (var iface in matchedInterfaces)
                    services.Add(new ServiceDescriptor(iface, impl, lifetime));
            }
        }

        private static void RegisterNotificationHandlers(
            IServiceCollection services,
            List<Type> types,
            ServiceLifetime lifetime)
        {
            var handlerInterface = typeof(INotificationHandler<>);

            foreach (var impl in types)
            {
                var matchedInterfaces = impl.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterface);

                foreach (var iface in matchedInterfaces)
                    services.Add(new ServiceDescriptor(iface, impl, lifetime));
            }
        }

        private static void RegisterPipelineBehaviors(
            IServiceCollection services,
            List<Type> types)
        {
            var behaviorInterface = typeof(IPipelineBehavior<,>);

            foreach (var impl in types)
            {
                var matchedInterfaces = impl.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == behaviorInterface);

                foreach (var iface in matchedInterfaces)
                    services.Add(new ServiceDescriptor(iface, impl, ServiceLifetime.Transient));
            }
        }

        private static List<Assembly> ResolveAssemblies(Assembly[]? assemblies)
        {
            if (assemblies is { Length: > 0 })
                return assemblies.Distinct().ToList();

            var list = new List<Assembly>();

            var entry = Assembly.GetEntryAssembly();
            if (entry is not null) list.Add(entry);

            // Entry yoksa (test ortamı gibi) calling assembly'ye düş
            if (list.Count == 0)
            {
                var calling = Assembly.GetCallingAssembly();
                list.Add(calling);
            }

            return list;
        }

        private static IEnumerable<Type> SafeGetTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(t => t is not null)!;
            }
            catch
            {
                return [];
            }
        }
    }
}
