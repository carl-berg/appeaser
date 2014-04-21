using System;

namespace Appeaser
{
    public interface IMediatorHandlerFactory
    {
        object GetHandler(Type handlerType);
    }
}