using System;
using System.Collections.Generic;

namespace Appeaser
{
    public interface IMediatorSettings
    {
        bool WrapExceptions { get; }
        IEnumerable<Type> RequestInterceptors { get; }
        IEnumerable<Type> ResponseInterceptors { get; }
    }
}
