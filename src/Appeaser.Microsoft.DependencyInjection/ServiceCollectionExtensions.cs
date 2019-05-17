﻿using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;

namespace Appeaser.Microsoft.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds Appeaser IMediator, ISimpleMediator and all handlers, in provided assemblies, to service collection
        /// </summary>
        /// <param name="assemblies">Assemblies containing handlers (if none provided, the calling assembly will be added by default)</param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static IServiceCollection AddAppeaser(this IServiceCollection services, params Assembly[] assemblies)
        {
            if (assemblies is null || !assemblies.Any())
            {
                assemblies = new[] { Assembly.GetCallingAssembly() };
            }

            return AddAppeaser(services, (MediatorSettings)null, assemblies);
        }

        /// <summary>
        /// Adds Appeaser IMediator, ISimpleMediator and all handlers, in provided assemblies, to service collection
        /// </summary>
        /// <param name="assemblies">Assemblies containing handlers (if none provided, the calling assembly will be added by default)</param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static IServiceCollection AddAppeaser(this IServiceCollection services, Action<IMediatorSettings> configure, params Assembly[] assemblies)
        {
            if (assemblies is null || !assemblies.Any())
            {
                assemblies = new[] { Assembly.GetCallingAssembly() };
            }

            var settings = new MediatorSettings();
            configure(settings);
            return AddAppeaser(services, settings, assemblies);
        }

        /// <summary>
        /// Adds Appeaser IMediator, ISimpleMediator and all handlers, in provided assemblies, to service collection
        /// </summary>
        /// <param name="assemblies">Assemblies containing handlers (if none provided, the calling assembly will be added by default)</param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static IServiceCollection AddAppeaser(this IServiceCollection services, IMediatorSettings settings, params Assembly[] assemblies)
        {
            if (assemblies is null || !assemblies.Any())
            {
                assemblies = new[] { Assembly.GetCallingAssembly() };
            }

            if (settings != null)
            {
                services.AddSingleton<IMediatorSettings>(settings);
            }
            
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
            var handlers = scanner.ResolveOpenTypes(openType);
            foreach (var handler in handlers)
            {
                services.AddTransient(handler.InterfaceType, handler.HandlerType);
            }
        }
    }
}
