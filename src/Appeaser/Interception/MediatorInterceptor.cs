using System;
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
                scope.InvokeRequestInterceptors();
                var result = invocation(scope);
                scope.InvokeResponseInterceptors(scope.CreateResponseInterceptionContext<TResponse>(result));
                return result;
            }
            catch (Exception ex)
            {
                if (ex is TargetInvocationException)
                {
                    scope.InvokeResponseInterceptors(scope.CreateExceptionInterceptionContext<TResponse>(ex.InnerException));
                    throw ex.InnerException;
                }
                else
                {
                    scope.InvokeResponseInterceptors(scope.CreateExceptionInterceptionContext<TResponse>(ex));
                }

                throw;
            }
        }

        public async Task<TResponse> InvokeHandlerAsync<TResponse>(object handler, object parameter, Func<MediatorInterceptionScope, Task<TResponse>> invocation)
        {
            var scope = new MediatorInterceptionScope(_config, handler, parameter);
            try
            {
                await scope.InvokeRequestInterceptorsAsync();
                var result = await invocation(scope);
                await scope.InvokeResponseInterceptorsAsync(scope.CreateResponseInterceptionContext<TResponse>(result));
                return result;
            }
            catch (Exception ex)
            {
                if (ex is TargetInvocationException)
                {
                    await scope.InvokeResponseInterceptorsAsync(scope.CreateExceptionInterceptionContext<TResponse>(ex.InnerException));
                    throw ex.InnerException;
                }
                else
                {
                    await scope.InvokeResponseInterceptorsAsync(scope.CreateExceptionInterceptionContext<TResponse>(ex));
                }

                throw;
            }
        }
    }
}
