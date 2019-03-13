using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Appeaser.Interception
{
    internal class MediatorInterceptionScope : IRequestInterceptionContext
    {
        private readonly MediatorInterceptionParameters _config;

        public MediatorInterceptionScope(MediatorInterceptionParameters config, object handler, object parameter)
        {
            _config = config;
            HandlerType = handler.GetType();
            RequestType = parameter.GetType();
            Context = new Dictionary<string, object> { { "Handler", handler } };
            Request = parameter;
        }

        public Type RequestType { get; }

        public Type HandlerType { get; }

        public object Request { get; }

        public IDictionary<string, object> Context { get; }

        internal async Task InvokeRequestInterceptorsAsync()
        {
            var interceptors = ResolveInterceptors<IRequestInterceptor>(x => x.RequestInterceptors);
            foreach (var interceptor in interceptors)
            {
                await interceptor.InterceptAsync(this).ConfigureAwait(false);
            }
        }

        internal void InvokeRequestInterceptors()
        {
            var interceptors = ResolveInterceptors<IRequestInterceptor>(x => x.RequestInterceptors);
            foreach (var interceptor in interceptors)
            {
                interceptor.Intercept(this);
            }
        }

        internal void InvokeResponseInterceptors(ResponseInterceptionContext context)
        {
            var interceptors = ResolveInterceptors<IResponseInterceptor>(x => x.ResponseInterceptors.Reverse());
            foreach (var interceptor in interceptors)
            {
                interceptor.Intercept(context);
            }
        }

        internal async Task InvokeResponseInterceptorsAsync(ResponseInterceptionContext context)
        {
            var interceptors = ResolveInterceptors<IResponseInterceptor>(x => x.ResponseInterceptors.Reverse());
            foreach (var interceptor in interceptors)
            {
                await interceptor.InterceptAsync(context);
            }
        }

        internal IEnumerable<T> ResolveInterceptors<T>(Func<MediatorInterceptionParameters, IEnumerable<Type>> typedefinitions)
        {
            return typedefinitions(_config).Select(type => _config.Resolve(type)).OfType<T>().ToList();
        }

        internal ResponseInterceptionContext CreateResponseInterceptionContext<TResponse>(object result)
        {
            return new ResponseInterceptionContext(this, typeof(TResponse), result);
        }

        internal ResponseInterceptionContext CreateExceptionInterceptionContext<TResponse>(Exception exception)
        {
            return new ResponseInterceptionContext(this, typeof(TResponse), exception);
        }
    }
}
