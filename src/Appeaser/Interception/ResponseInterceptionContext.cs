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
        public ResponseInterceptionContext(IRequestInterceptionContext requestContext, Type responseType, object response) : this(requestContext)
        {
            ResponseType = responseType;
            Response = response;
        }

        public ResponseInterceptionContext(IRequestInterceptionContext requestContext, Type responseType, Exception exception) : this(requestContext)
        {
            ResponseType = responseType;
            Exception = exception;
        }

        private ResponseInterceptionContext(IRequestInterceptionContext requestContext)
        {
            HandlerType = requestContext.HandlerType;
            RequestType = requestContext.RequestType;
            HandlerInstance = requestContext.HandlerInstance;
            Request = requestContext.Request;
        }

        public Type ResponseType { get; }

        public Exception Exception { get; }

        public object Response { get; set; }

        public Type RequestType { get; }

        public Type HandlerType { get; }

        public object HandlerInstance { get; }

        public object Request { get; }
    }
}
