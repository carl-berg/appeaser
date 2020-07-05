using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Appeaser.Interception
{
    internal class MediatorInterceptor
    {
        private readonly MediatorInterceptionParameters _config;

        public MediatorInterceptor(IMediatorSettings settings, IMediatorHandlerFactory handlerFactory)
        {
            _config = new MediatorInterceptionParameters(settings.RequestInterceptors, settings.ResponseInterceptors, handlerFactory);
        }

        public TResponse InvokeHandler<TResponse>(object handler, object parameter, Func<MediatorInterceptionScope, TResponse> invocation)
        {
            var scope = new MediatorInterceptionScope(_config, handler, parameter);
            try
            {
                foreach (var interceptor in ResolveInterceptors<IRequestInterceptor>(x => x.RequestInterceptors))
                {
                    interceptor.Intercept(scope);
                }

                var result = invocation(scope);

                foreach (var interceptor in ResolveInterceptors<IResponseInterceptor>(x => x.ResponseInterceptors).Reverse())
                {
                    interceptor.Intercept(scope.CreateResponseInterceptionContext<TResponse>(result));
                }

                return result;
            }
            catch (Exception ex)
            {
                var exception = ex is TargetInvocationException ? ex.InnerException : ex;
                foreach (var interceptor in ResolveInterceptors<IResponseInterceptor>(x => x.ResponseInterceptors).Reverse())
                {
                    interceptor.Intercept(scope.CreateExceptionInterceptionContext<TResponse>(exception));
                }

                throw exception;
            }
        }

        public async Task<TResponse> InvokeHandlerAsync<TResponse>(object handler, object parameter, Func<MediatorInterceptionScope, Task<TResponse>> invocation)
        {
            var scope = new MediatorInterceptionScope(_config, handler, parameter);
            try
            {
                foreach (var interceptor in ResolveInterceptors<IRequestInterceptor>(x => x.RequestInterceptors))
                {
                    await interceptor.InterceptAsync(scope);
                }

                var result = await invocation(scope);

                foreach (var interceptor in ResolveInterceptors<IRequestInterceptor>(x => x.ResponseInterceptors).Reverse())
                {
                    await interceptor.InterceptAsync(scope.CreateResponseInterceptionContext<TResponse>(result));
                }

                return result;
            }
            catch (Exception ex)
            {
                var exception = ex is TargetInvocationException ? ex.InnerException : ex;
                foreach (var interceptor in ResolveInterceptors<IRequestInterceptor>(x => x.ResponseInterceptors).Reverse())
                {
                    await interceptor.InterceptAsync(scope.CreateExceptionInterceptionContext<TResponse>(exception));
                }

                throw exception;
            }
        }

        private IEnumerable<T> ResolveInterceptors<T>(Func<MediatorInterceptionParameters, IEnumerable<Type>> typedefinitions)
        {
            foreach (var interceptorType in typedefinitions(_config))
            {
                if (interceptorType.GetTypeInfo().ImplementedInterfaces.Contains(typeof(T)))
                {
                    if (_config.Resolve(interceptorType) is T interceptor)
                    {
                        yield return interceptor;
                    }
                    else
                    {
                        throw new MediatorInterceptionResolutionException(interceptorType);
                    }
                }
            }
        }
    }
}
