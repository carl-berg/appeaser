using System;
using System.Collections.Generic;

namespace Appeaser.Interception
{
    internal class MediatorInterceptionParameters
    {
        public MediatorInterceptionParameters(IEnumerable<Type> requestInterceptors, IEnumerable<Type> responseInterceptors, IMediatorHandlerFactory handlerFactory)
        {
            RequestInterceptors = requestInterceptors;
            ResponseInterceptors = responseInterceptors;
            Resolve = ResolveResolver(handlerFactory);
        }

        public IEnumerable<Type> RequestInterceptors { get; }
        public IEnumerable<Type> ResponseInterceptors { get; }
        public Func<Type, object> Resolve { get; }

        private Func<Type, object> ResolveResolver(IMediatorHandlerFactory handlerFactory)
        {
            if (handlerFactory is IMediatorResolver resolver)
            {
                return resolver.GetInterceptor;
            }

            return handlerFactory.GetHandler;
        }
    }
}
