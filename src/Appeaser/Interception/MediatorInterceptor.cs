using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Appeaser.Interception
{
    internal class MediatorInterceptor
    {
        private static readonly TaskFactory TaskFactory = new TaskFactory(CancellationToken.None,
            TaskCreationOptions.None,
            TaskContinuationOptions.None,
            TaskScheduler.Default);
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
                RunSync(() => scope.InvokeRequestInterceptors());
                var result = invocation(scope);
                RunSync(() => scope.InvokeResponseInterceptors(result));
                return result;
            }
            catch (Exception ex)
            {
                if (ex is TargetInvocationException)
                {
                    RunSync(() => scope.InvokeResponseInterceptorsWithException<TResponse>(ex.InnerException));
                    throw ex.InnerException;
                }
                else
                {
                    RunSync(() => scope.InvokeResponseInterceptorsWithException<TResponse>(ex));
                }

                throw;
            }
        }

        public async Task<TResponse> InvokeHandlerAsync<TResponse>(object handler, object parameter, Func<MediatorInterceptionScope, Task<TResponse>> invocation)
        {
            var scope = new MediatorInterceptionScope(_config, handler, parameter);
            try
            {
                await scope.InvokeRequestInterceptors();
                var result = await invocation(scope);
                await scope.InvokeResponseInterceptors(result);
                return result;
            }
            catch (Exception ex)
            {
                if (ex is TargetInvocationException)
                {
                    await scope.InvokeResponseInterceptorsWithException<TResponse>(ex.InnerException);
                    throw ex.InnerException;
                }
                else
                {
                    await scope.InvokeResponseInterceptorsWithException<TResponse>(ex);
                }

                throw;
            }
        }

        private static void RunSync(Func<Task> func)
        {
            TaskFactory
                .StartNew(func)
                .Unwrap()
                .GetAwaiter()
                .GetResult();
        }
    }
}
