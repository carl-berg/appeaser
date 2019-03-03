using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Appeaser.Interception
{
    internal class MediatorInterceptionScope : IRequestInterceptionContext
    {
        private readonly MediatorInterceptionParameters _config;
        private IEnumerable<IRequestInterceptor> _requestInterceptors;

        public MediatorInterceptionScope(MediatorInterceptionParameters config, object handler, object parameter)
        {
            _config = config;
            HandlerType = handler.GetType();
            RequestType = parameter.GetType();
            HandlerInstance = handler;
            Request = parameter;
        }

        public Type RequestType { get; }

        public Type HandlerType { get; }

        public object HandlerInstance { get; }

        public object Request { get; }

        internal async Task InvokeRequestInterceptors()
        {
            _requestInterceptors = _config.RequestInterceptors
                .Select(type => _config.Resolve(type))
                .OfType<IRequestInterceptor>().ToList();
            foreach (var interceptor in _requestInterceptors)
            {
                await interceptor.Intercept(this);
            }
        }

        internal async Task InvokeResponseInterceptors<TResponse>(TResponse response)
        {
            var context = new ResponseInterceptionContext(this, typeof(TResponse), response);
            var interceptors = _config.ResponseInterceptors
                .Select(ResolveResponseInterceptor)
                .OfType<IResponseInterceptor>().ToList();
            foreach (var interceptor in interceptors)
            {
                await interceptor.Intercept(context);
            }
        }

        internal async Task InvokeResponseInterceptorsWithException<TResponse>(Exception exception)
        {
            var context = new ResponseInterceptionContext(this, typeof(TResponse), exception);
            var interceptors = _config.ResponseInterceptors
                .Select(ResolveResponseInterceptor)
                .OfType<IResponseInterceptor>().ToList();
            foreach (var interceptor in interceptors)
            {
                await interceptor.Intercept(context);
            }
        }

        private IResponseInterceptor ResolveResponseInterceptor(Type type)
        {
            var matchingRequestInterceptor = _requestInterceptors
                .SingleOrDefault(requestInterceptor => requestInterceptor.GetType() == type);

            if (matchingRequestInterceptor != null)
            {
                return matchingRequestInterceptor as IResponseInterceptor;
            }

            return _config.Resolve(type) as IResponseInterceptor;
        }
    }
}
