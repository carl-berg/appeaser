using System;

namespace Appeaser.Interception
{
    public interface IResponseInterceptionContext : IRequestInterceptionContext
    {
        Type ResponseType { get; }
        Exception Exception { get; }
        object Response { get; set; }
    }

    internal class ResponseInterceptionContext : IRequestInterceptionContext, IResponseInterceptionContext
    {
        public ResponseInterceptionContext(MediatorInterceptionScope requestContext, Type responseType, object response) : this(requestContext)
        {
            ResponseType = responseType;
            Response = response;
        }

        public ResponseInterceptionContext(MediatorInterceptionScope requestContext, Type responseType, Exception exception) : this(requestContext)
        {
            ResponseType = responseType;
            Exception = exception;
        }

        private ResponseInterceptionContext(MediatorInterceptionScope requestContext)
        {
            HandlerType = requestContext.HandlerType;
            RequestType = requestContext.RequestType;
            Context = requestContext.Context;
            Request = requestContext.Request;
        }

        public Type ResponseType { get; }

        public Exception Exception { get; }

        public object Response { get; set; }

        public Type RequestType { get; }

        public Type HandlerType { get; }

        public object Request { get; }

        internal IContext Context { get; }

        public object Get(string key) => Context.Get(key);

        public T Get<T>(string key) => Context.Get<T>(key);

        public void Set<T>(string key, T value) => Context.Set(key, value);
    }
}
