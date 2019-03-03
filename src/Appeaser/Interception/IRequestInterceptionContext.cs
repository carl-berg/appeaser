using System;

namespace Appeaser.Interception
{
    public interface IRequestInterceptionContext
    {
        Type RequestType { get; }
        Type HandlerType { get; }
        object HandlerInstance { get; }
        object Request { get; }
    }
}
