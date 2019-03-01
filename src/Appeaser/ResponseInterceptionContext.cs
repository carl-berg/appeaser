using System;

namespace Appeaser
{
    public interface IResponseInterceptionContext : IRequestInterceptionContext
    {
        Type ResponseType { get; }
        Exception Exception { get; }
        object Response { get; set; }
    }

    internal class ResponseInterceptionContext : RequestInterceptionContext, IResponseInterceptionContext
    {
        public ResponseInterceptionContext(RequestInterceptionContext requestContext, Type responseType, object response) : base(requestContext)
        {
            ResponseType = responseType;
            Response = response;
        }

        public ResponseInterceptionContext(RequestInterceptionContext requestContext, Type responseType, Exception exception) : base(requestContext)
        {
            ResponseType = responseType;
            Exception = exception;
        }

        public Type ResponseType { get; }

        public Exception Exception { get; }

        public object Response { get; set; }
    }
}
