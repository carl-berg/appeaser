using System;

namespace Appeaser.Microsoft.DependencyInjection
{
    internal class MicrosoftDependencyInjectionMediatorResolver : IMediatorResolver
    {
        private readonly IServiceProvider _serviceProvider;

        public MicrosoftDependencyInjectionMediatorResolver(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public object GetHandler(Type handlerType)
        {
            return _serviceProvider.GetService(handlerType);
        }

        public object GetInterceptor(Type interceptorType)
        {
            return _serviceProvider.GetService(interceptorType);
        }
    }
}
