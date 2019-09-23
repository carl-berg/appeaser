using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using ATypeScanner;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Appeaser.Microsoft.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds Appeaser IMediator, ISimpleMediator and all handlers, in provided assemblies, to service collection
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="configureSettings">Configure mediator settings</param>
        /// <param name="assemblies">Assemblies containing handlers (if none provided, the calling assembly will be added by default)</param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static IServiceCollection AddAppeaser(this IServiceCollection services, Action<MediatorSettings> configureSettings = null, params Assembly[] assemblies)
        {
            if (assemblies is null || !assemblies.Any())
            {
                assemblies = new[] { Assembly.GetCallingAssembly() };
            }

            services.AddOptions<MediatorSettings>();

            if (configureSettings != null)
            {
                services.Configure(configureSettings);
            }
            
            services.AddScoped<IMediatorSettings>(s => s.GetService<IOptions<MediatorSettings>>().Value);
            services.AddScoped<IMediatorResolver, MicrosoftDependencyInjectionMediatorResolver>();
            services.AddScoped<IMediator, Mediator>();
            services.AddScoped<ISimpleMediator, Mediator>();

            var scanner = new TypeScanner(assemblies);

            FindAndRegistersHandlersOfOpenType(scanner, services, typeof(IRequestHandler<,>));
            FindAndRegistersHandlersOfOpenType(scanner, services, typeof(IQueryHandler<,>));
            FindAndRegistersHandlersOfOpenType(scanner, services, typeof(ICommandHandler<,>));

            FindAndRegistersHandlersOfOpenType(scanner, services, typeof(IAsyncRequestHandler<,>));
            FindAndRegistersHandlersOfOpenType(scanner, services, typeof(IAsyncQueryHandler<,>));
            FindAndRegistersHandlersOfOpenType(scanner, services, typeof(IAsyncCommandHandler<,>));

            return services;
        }


        private static void FindAndRegistersHandlersOfOpenType(TypeScanner scanner, IServiceCollection services, Type openType)
        {
            var handlers = scanner.FindClosingImplementationsOf(openType);
            foreach (var handler in handlers)
            {
                services.AddTransient(handler.GenericType, handler.ConcreteType);
            }
        }
    }
}
