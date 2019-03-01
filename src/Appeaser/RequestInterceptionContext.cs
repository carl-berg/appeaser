using System;

namespace Appeaser
{
    public interface IRequestInterceptionContext
    {
        Type RequestType { get; }
        Type HandlerType { get; }
        object HandlerInstance { get; }
        object Request { get; }
    }

    internal class RequestInterceptionContext : IRequestInterceptionContext
    {
        public RequestInterceptionContext(Type requestType, Type handlerType, object handlerInstance, object request)
        {
            RequestType = requestType;
            HandlerType = handlerType;
            HandlerInstance = handlerInstance;
            Request = request;
        }

        public RequestInterceptionContext(RequestInterceptionContext context)
            : this(context.RequestType, context.HandlerType, context.HandlerInstance, context.Request)
        { }

        public Type RequestType { get; }
        public Type HandlerType { get; }

        public object HandlerInstance { get; }

        public object Request { get; }
    }
}
