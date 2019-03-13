using System;
using System.Collections.Generic;

namespace Appeaser.Interception
{
    public interface IRequestInterceptionContext : IContext
    {
        Type RequestType { get; }
        Type HandlerType { get; }
        object Request { get; }
    }
}
