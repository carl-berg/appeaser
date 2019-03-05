using System;
using System.Collections.Generic;

namespace Appeaser.Interception
{
    public interface IRequestInterceptionContext
    {
        Type RequestType { get; }
        Type HandlerType { get; }
        object Request { get; }
        IDictionary<string, object> Context { get; }
    }
}
