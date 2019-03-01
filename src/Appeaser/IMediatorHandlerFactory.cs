using System;

namespace Appeaser
{
    public interface IMediatorHandlerFactory
    {
        object GetHandler(Type handlerType);
    }

    public interface IMediatorResolver : IMediatorHandlerFactory
    {
        object GetInterceptor(Type interceptorType);
    }
}